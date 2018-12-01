autowatch = 1;
inlets = 2;
outlets = 2;

var v_low;
function volume_low(val) {
	v_low = val;
}
var x_c, y_c;
function x_center(val) {
	x_c = val;
}
function y_center(val) {
	y_c = val;
}
var z_l, z_h, z_c;
function z_low(val) {
	z_l = val;
}
function z_high(val) {
	z_h = val;
}
function z_center(val) {
	z_c = val;
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
function scale_frequency(l_freq, r_freq, l_amp, r_amp) {
	var val;
	if (l_freq < 1) {
		val = r_freq;
	}
	else if (r_freq < 1) {
		val = l_freq;
	}
	else if (l_amp < 0.001) {
		val = r_freq;
	}
	else if (r_amp < 0.001) {
		val = l_freq;
	}
	else if (l_amp > r_amp) {
		val = l_freq;
	}
	else {
		val = r_freq;
	}
	this.patcher.getnamed("frequency_in").message("float", val);
    var objOut = this.patcher.getnamed("frequency_out");
	return objOut.getvalueof();
}

var ampstats_max, ampstats_min;
ampstats.immediate = 1;
function ampstats() {
	if (peaks === 0) {
		return;
	}
	if (arguments.length != 4) {
		return;
	}
	// list: left_min left_max right_min right_max
	var l_min = Math.abs(arguments[0]);
	var l_max = Math.abs(arguments[1]);
	var r_min = Math.abs(arguments[2]);
	var r_max = Math.abs(arguments[3]);
	ampstats_max = Math.max(l_min, l_max, r_min, r_max);
	ampstats_min = Math.min(l_min, l_max, r_min, r_max);
}

var freqpeak_l = [];
var freqpeak_r = [];
list.immediate = 1;
function list() {
	if (peaks === 0) {
		return;
	}
	if (inlet === 0 && arguments.length) {
		freqpeak_l = arguments;
	}
	if (inlet === 1 && arguments.length) {
		freqpeak_r = arguments;
	}
	if (freqpeak_l.length === freqpeak_r.length) {
		getpoints(freqpeak_l, freqpeak_r, true, true);
		freqpeak_l = [];
		freqpeak_r = [];
	}
	else if (inlet === 0) {
		freqpeak_r = [];
	}
	else if (inlet === 1) {
		freqpeak_l = [];
	}
}

getpoints.local = 1;
function getpoints(list_l, list_r, sort, fill) {
	if (list_l.length === 0 || Math.abs(list_l.length % 2) == 1) {
		return;
	}
	if (typeof ampstats_max === 'undefined') {
		return;
	}
	else if (ampstats_max === 0) {
		return;
	}
	else if (ampstats_max < v_low) {
		return;
	}
	var tmp_peaks = peaks;
	var tmp_ampstats_max = ampstats_max;
	var tmp_ampstats_min = ampstats_min;
	// each pair of arguments is freq1 amp1 freq2 amp2...
	// first half of list is left audio, last half is right audio
	var res = {};
	var id = 0;
	var x,y,z;
	var amp_max = 0;
	var amp_min = 1;
	for(var i = 0; i < list_l.length; i++) {
		var l_freq = parseFloat(list_l[i]);
		var l_amp = parseFloat(list_l[i+1]);
		var r_freq = parseFloat(list_r[i]);
		var r_amp = parseFloat(list_r[i+1]);
		if (l_amp < 0.001 && r_amp < 0.001) {
			i++;
			continue;
		}
		if (l_freq < 1 && r_freq < 1) {
			i++;
			continue;
		}
		x = get_pan_position(l_amp, r_amp);
		y = scale_frequency(l_freq, r_freq, l_amp, r_amp);
		// temporary z
		z = Math.max(l_amp, r_amp);
		if (z > amp_max) {
			amp_max = z;
		}
		else if (z < amp_min) {
			amp_min = z;
		}
		res[id] = [parseFloat(x),parseFloat(y),z];
		id++;
		i++;
	}
	if (Object.keys(res).length === 0) {
		return;
	}
	// fix relative Z + index
	var res_index = [];
	for (var key in res) {
		// scale formula = c + (d-c)*((v-a)/(b-a))
		z = tmp_ampstats_min + (tmp_ampstats_max-tmp_ampstats_min)*((res[key][2]-amp_min)/(amp_max-amp_min));
		this.patcher.getnamed("volume_in").message("float", z);
	    var objOut = this.patcher.getnamed("volume_out");
		z = objOut.getvalueof();
		res[key][2] = parseFloat(z);
		res_index.push([key,parseFloat(z)]);
	}
	if (sort) {
		res_index.sort(function(a, b){
			if (z_h < z_l) {
				return a[1] - b[1];
			}
			else {
				return b[1] - a[1];
			}
		});
	}
	if (fill && res_index.length < tmp_peaks) {
		if (res_index.length === 1) {
			res[id] = [parseFloat(x_c),parseFloat(y_c),parseFloat(z_c)];
			res_index.push([id,parseFloat(z_c)]);
			id++;
		}
		var pointStart = [];
		var pointEnd = [];
		i = 0;
		while (res_index.length < tmp_peaks) {
			pointStart = res[res_index[i][0]];
			pointEnd = res[res_index[i+1][0]];
			x = (pointStart[0] + pointEnd[0]) / 2;
			y = (pointStart[1] + pointEnd[1]) / 2;
			z = (pointStart[2] + pointEnd[2]) / 2;
			res[id] = [x,y,z];
			res_index.push([id,z]);
			id++;
			i++;
		}
		if (sort) {
			res_index.sort(function(a, b){
				if (z_h < z_l) {
					return a[1] - b[1];
				}
				else {
					return b[1] - a[1];
				}
			});
		}
	}
	for(var point = 0; point < tmp_peaks; point++) {
		if (!res_index[point]) {
			break;
		}
		outlet(0, "/"+point, res[res_index[point][0]][0], res[res_index[point][0]][1], res[res_index[point][0]][2]);
	}
	outlet(1, "bang");
}