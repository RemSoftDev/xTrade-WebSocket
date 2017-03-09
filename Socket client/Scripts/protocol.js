function ord(string) {
	var str = string + '',
		code = str.charCodeAt(0);
	if (0xD800 <= code && code <= 0xDBFF) {
		var hi = code;
		if (str.length === 1) {
			return code;
		}
		var low = str.charCodeAt(1);
		return ((hi - 0xD800) * 0x400) + (low - 0xDC00) + 0x10000;
	}
	if (0xDC00 <= code && code <= 0xDFFF) {
		return code;
	}
	return code;
}

function chr(codePt) {
	if (codePt > 0xFFFF) {
		codePt -= 0x10000;
		return String.fromCharCode(0xD800 + (codePt >> 10), 0xDC00 + (codePt & 0x3FF));
	}
	return String.fromCharCode(codePt);
}
// "client" "json string"
function Protocol(side, manifest) {
	var $this = this;
	this.side = null;
	this.manifest = null;
	this.error = null;
	this.readBuffer = null;
	this.readPos = null;
	this.writeBuffer = null;
	this.writePos = null;
	this.sizeOf = {
		'bool': 1,
		'byte': 8,
		'ubyte': 8,
		'int16': 16,
		'uint16': 16,
		'int32': 32,
		'uint32': 32,
		'float': 32,
		'double': 64,
		'currency': 32,
		'char': 8,
		'date': 23,
		'datetime': 40,
		'time': 17
	};

	this.construct = function (side, manifest) {
		if (!$this.setManifest(manifest)) {
			alert($this.getLastError());
		}
		if ($this.manifest[side] === undefined) {
			alert('Section ' + side + ' is not declared in manifest.');
		}
		$this.side = side;
	};

	this.getManifest = function () {
		return $this.manifest;
	};

	this.setManifest = function (manifest) {
		if (typeof manifest !== "object") {
			manifest = JSON.parse(manifest);
		}

		if (!$this.verifyManifest(manifest)) {
			$this.manifest = null;
			return false;
		}
		$this.manifest = manifest;
		return true;
	};

	this.verifyManifest = function (manifest) {
		return true;
	};

	this.getLastError = function () {
		return $this.error;
	};

	this.getCommandById = function (cmdId, side) {
		if ($this.manifest[side] === undefined) return null;
		var cmds = $this.manifest[side];
		for (var i = 0; i < cmds.length; i++) {
			var cmd = cmds[i];
			if (cmd.id === cmdId) return cmd;
		}
		return null;
	};

	this.getCommandByName = function (cmdName, side) {
		if ($this.manifest[side] === undefined) return null;
		var cmds = $this.manifest[side];
		for (var i = 0; i < cmds.length; i++) {
			var cmd = cmds[i];
			if (cmd.name === cmdName) return cmd;
		}
		return null;
	};

	this.formatNumber = function (num, digits) {
		num = num.toString();
		for (var i = num.length; i < digits; i++) num = '0' + num;
		return num;
	}

	this.binaryToFloat = function(binary) {
		var s = ord(binary[3]) >> 7;
		var e = (ord(binary[3]) << 8 | ord(binary[2])) >> 7 & 0xff;
		var m = (ord(binary[2]) & 0x7f) << 16 | ord(binary[1]) << 8 | ord(binary[0]);
		switch (e) {
			case 0: return 0.0; break;
			case 0xff: return m ? NaN : (s ? Number.POSITIVE_INFINITY : Number.NEGATIVE_INFINITY); break;
			default: return (s ? -1 : 1) * (1 + m / 8388608.0) * Math.pow(2, e - 127);
		}
	}

	this.binaryToDouble = function(binary) {
		var s = ord(binary[7]) >> 7;
		var e = (ord(binary[7]) << 8 | ord(binary[6])) >> 4 & 0x07ff;
		var m1 = (ord(binary[6]) & 0x0f) << 16 | ord(binary[5]) << 8 | ord(binary[4]);
		var m2 = (ord(binary[3]) << 24 | ord(binary[2]) << 16 | ord(binary[1]) << 8 | ord(binary[0])) >>> 0;
		var m = m1 * 4294967296.0 + m2;
		switch (e) {
			case 0: return 0.0; break;
			case 0x07ff: return m ? NaN : (s ? Number.POSITIVE_INFINITY : Number.NEGATIVE_INFINITY); break;
			default: return (s ? -1 : 1) * (1 + m / 4503599627370496.0) * Math.pow(2, e - 1023);
		}
	}

// Decoder methods -----------------------------------------------------------------------------------------------------

	this.binaryToValue = function(type, value) {
		var result;
		switch (type) {
			case 'bool': result = ord(value[0]) ? true : false; break;
			case 'byte': result = ord(value[0]); if (result > 127) result -= 256; break;
			case 'ubyte': result = ord(value[0]); break;
			case 'int16': result = (ord(value[0]) << 8) | ord(value[1]); if (result > 32767) result -= 65536; break;
			case 'uint16': result = (ord(value[0]) << 8) | ord(value[1]); break;
			case 'int32': result = (ord(value[0]) << 24) | (ord(value[1]) << 16) | (ord(value[2]) << 8) | ord(value[3]); if (result > 2147483647) result -= 4294967296; break;
			case 'uint32': result = ((ord(value[0]) << 24) | (ord(value[1]) << 16) | (ord(value[2]) << 8) | ord(value[3])) >>> 0; break;
			case 'float': result = $this.binaryToFloat(value); break;
			case 'double': result = $this.binaryToDouble(value); break;
			case 'currency': result = ((ord(value[0]) << 24) | (ord(value[1]) << 16) | (ord(value[2]) << 8) | ord(value[3])) / 100; break;
			case 'char': result = value; break;
			case 'date':
				value = (ord(value[0]) << 16) | (ord(value[1]) << 8) | ord(value[2]);
				var day = value & 0x1f;               // 5-bit length
				var month = (value >> 5) & 0x0f;      // 4-bit length
				var year = (value >> 9) & 0x3fff;     // 14-bit length
				result = $this.formatNumber(year, 4) + '-' + $this.formatNumber(month, 2) + '-' + $this.formatNumber(day, 2);
				break;
			case 'datetime':
				var val = (ord(value[2]) << 16) | (ord(value[3]) << 8) | ord(value[4]);
				var s = val & 0x3f;                   // 6-bit length
				var m = (val >> 6) & 0x3f;            // 6-bit length
				var h = (val >> 12) & 0x1f;           // 5-bit length

				val = (ord(value[0]) << 16) | (ord(value[1]) << 8) | ord(value[2]);
				day = (val >> 1) & 0x1f;          // 5-bit length
				month = (val >> 6) & 0x0f;        // 4-bit length
				year = (val >> 10) & 0x3fff;      // 14-bit length

				var time = h + ':' + $this.formatNumber(m, 2) + ':' + $this.formatNumber(s, 2);
				var date = $this.formatNumber(year, 4) + '-' + $this.formatNumber(month, 2) + '-' + $this.formatNumber(day, 2);
				result = date + ' ' + time;
				break;
			case 'time':
				value = (ord(value[0]) << 16) | (ord(value[1]) << 8) | ord(value[2]);
				s = value & 0x3f;                 // 6-bit length
				m = (value >> 6) & 0x3f;          // 6-bit length
				h = (value >> 12) & 0x1f;         // 5-bit length
				result = h + ':' + $this.formatNumber(m, 2) + ':' + $this.formatNumber(s, 2);
				break;
			default: result = null;
		}
		return result;
	};

	this.read = function(len) {
		var result = '';
		while (len > 0) {
			var bytePos = Math.floor($this.readPos / 8);              // byte pos where to look for our data
			var toRead = (len % 8) ? (len % 8) : 8;                   // bits to read for this iteration
			var offset = (8 - $this.readPos % 8) + (8 - toRead);      // to move bits into byte boundaries (right-aligned)
			var byte1 = (($this.readBuffer[bytePos]) ? ord($this.readBuffer[bytePos]) : 0);
			var byte2 = (($this.readBuffer[bytePos + 1]) ? ord($this.readBuffer[bytePos + 1]) : 0);
			var value = (byte1 << 8) | byte2;
			value = (value >> offset) & 0xff;
			if (toRead < 8) value = ((value << (8 - toRead)) & 0xff) >> (8 - toRead);
			result += chr(value);

			len -= toRead;
			$this.readPos += toRead;
		}
		return result;
	};

	this.readValue = function(type) {
		var value;
		if (type != 'string') {
			var raw = $this.read($this.sizeOf[type]);
			value = $this.binaryToValue(type, raw);
		}
		else {
			value = '';
			var fin = false;
			do {
				var code = ord($this.read($this.sizeOf['char']));
				if (!code) fin = true;
				else {
					if (code > 127) {
						var len = 0;
						do {
							code = code << 1 & 0xff;
							len++;
						} while (code & 0x80);
						if (len > 1) {
							code >>= len;
							var bytes = $this.read((len - 1) * $this.sizeOf['char']);
							for (var i = 0; i < bytes.length; i++) {
								code = (code << 6) | ord(bytes[i]) & 0x3f;
							}
						}
					}
					value += chr(code);
				}
			} while (!fin);
		}
		return value;
	};

	this.decodeData = function(pattern) {
		var result;
		if (typeof pattern === "object" && pattern.length === undefined) {
			result = {};
			for (var key in pattern) {
				var format = pattern[key];
				var exists = $this.readValue('bool');
				if (!exists) {
					continue;
				}
				if (typeof format === "string") {
					result[key] = $this.readValue(format);
				}
				else {
					result[key] = $this.decodeData(format);
				}
			}
		}
		if (typeof pattern === "object" && pattern.length !== undefined) {
			result = [];
			var count = $this.readValue('uint32');
			for (var i = 0; i < count; i++) {
				result[i] = $this.decodeData(pattern[0]);
			}
		}
		return result;
	};

	this.decode = function (data) {
	    debugger
		$this.readBuffer = data;
		$this.readPos = 0;
		var side = ($this.side == 'client') ? 'server' : 'client';
		debugger
		var cmd = $this.getCommandById($this.readValue('byte'), side);
		debugger

		if (!cmd) return null;
		debugger
		var result = $this.decodeData(cmd.data);
		result = {
			action: cmd.name,
			data: result
		};

        debugger
		return result;
	};

	this.construct(side, manifest);
}