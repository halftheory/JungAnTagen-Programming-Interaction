autowatch = 1;
inlets = 1;
outlets = 1;

var f_low, f_mid, f_high;
function frequency_low(val) {
	f_low = val;
}
function frequency_mid(val) {
	f_mid = val;
}
function frequency_high(val) {
	f_high = val;
}
var v_low, v_mid, v_high;
function volume_low(val) {
	v_low = val;
}
function volume_mid(val) {
	v_mid = val;
}
function volume_high(val) {
	v_high = val;
}
var z_l, z_h;
function z_low(val) {
	z_l = val;
}
function z_high(val) {
	z_h = val;
}
var level = 1;
function set_level(val) {
	level = val;
}
var peaks = 1;
function set_peaks(val) {
	peaks = val;
}

get_pan_position.local = 1;
function get_pan_position(l_amp, r_amp) {
	l_amp = Math.abs(l_amp);
	r_amp = Math.abs(r_amp);
	var val = 0.5;
	if (l_amp == r_amp) {
		val = 0.5;
	}
	else if (l_amp === 0) {
		val = 1;
	}
	else if (r_amp === 0) {
		val = 0;
	}
	else if (l_amp > r_amp) {
		val = r_amp / l_amp * 0.5;
	}
	else if (r_amp > l_amp) {
		val = 1 - (l_amp / r_amp * 0.5);
	}
	this.patcher.getnamed("pan_in").message("float", val);
    var objOut = this.patcher.getnamed("pan_out");
	return objOut.getvalueof();
}

scale_frequency.local = 1;
function scale_frequency(val) {
	this.patcher.getnamed("frequency_in").message("float", val);
    var objOut = this.patcher.getnamed("frequency_out");
	return objOut.getvalueof();
}

var z_ampstats;
ampstats.immediate = 1;
function ampstats() {
	if (arguments.length != 4) {
		return;
	}
	if (level === 0) {
		return;
	}
	if (peaks === 0) {
		return;
	}
	// list: left_min left_max right_min right_max
	var l_min = Math.abs(arguments[0]);
	var l_max = arguments[1];
	var r_min = Math.abs(arguments[2]);
	var r_max = arguments[3];
	var val = Math.max(l_min, l_max, r_min, r_max);
	this.patcher.getnamed("volume_in").message("float", val);
    var objOut = this.patcher.getnamed("volume_out");
	z_ampstats = objOut.getvalueof();
}

freqpeak.immediate = 1;
function freqpeak() {
	getpoints(arguments, false);
}

freqpeak_sort.immediate = 1;
function freqpeak_sort() {
	getpoints(arguments, true);
}

getpoints.local = 1;
function getpoints(arguments, sort) {
	if (level === 0) {
		return;
	}
	if (peaks === 0) {
		return;
	}
	else if (peaks == 1) {
		sort = false;
	}
	if (arguments.length === 0 || Math.abs(arguments.length % 2) == 1) {
		return;
	}
	if (typeof z_ampstats === 'undefined') {
		return;
	}
	// each pair of arguments is freq1 amp1 freq2 amp2...
	// first half of list is left audio, last half is right audio
	var res = {};
	var res_index = [];
	var id = 0;
	var x,y,z;
	for(var i = 0; i < arguments.length / 2; i++) {
		var l_freq = parseFloat(arguments[i]);
		var l_amp = parseFloat(arguments[i + 1]);
		var r_freq = parseFloat(arguments[i + arguments.length / 2]);
		var r_amp = parseFloat(arguments[i + 1 + arguments.length / 2]);
		if (l_amp === 0 && r_amp === 0) {
			i++;
			continue;
		}
		if (l_freq === 0 && r_freq === 0) {
			i++;
			continue;
		}
		x = get_pan_position(l_amp, r_amp);
		if (l_amp > r_amp) {
			y = scale_frequency(l_freq);
		}
		else {
			y = scale_frequency(r_freq);
		}
		z = z_ampstats; // todo: refresh this?
		if (!sort) {
			outlet(0, id, x, y, z);
			if ((id + 1) >= peaks) {
				return;
			}
		}
		else {
			res[id] = [x,y,z];
			res_index.push([id,z]);
		}
		id++;
		i++;
	}
	if (!sort) {
		return;
	}
	if (res_index.length === 0) {
		return;
	}
	res_index.sort(function(a, b){
		if (z_h < z_l) {
			return a[1] - b[1];
		}
		else {
			return b[1] - a[1];
		}
	});
	for(var point = 0; point < peaks; point++) {
		if (!res_index[point]) {
			break;
		}
		outlet(0, point, res[res_index[point][0]][0], res[res_index[point][0]][1], res[res_index[point][0]][2]);
	}
}