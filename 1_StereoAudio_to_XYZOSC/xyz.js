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
var z_scale = 1;
function zscale(val) {
	z_scale = val;
}

function get_pan_position(l_amp, r_amp) {
	l_amp = Math.abs(l_amp);
	r_amp = Math.abs(r_amp);
	if (l_amp === 0) {
		return 1;
	}
	else if (r_amp === 0) {
		return 0;
	}
	else if (l_amp > r_amp) {
		return r_amp / l_amp * 0.5;
	}
	else if (r_amp > l_amp) {
		return 1 - (l_amp / r_amp * 0.5);
	}
	return 0.5;
}

function scale_frequency(val) {
	if (val <= f_low) {
		return 0;
	}
	else if (val >= f_high) {
		return 1;
	}
	else if (val < f_mid) {
		return (val - f_low) / (f_mid - f_low) * 0.5;
	}
	else if (val > f_mid) {
		return (val - f_mid) / (f_high - f_mid) * 0.5 + 0.5;
	}
	return 0.5;
}

function scale_volume(val) {
	if (val <= v_low) {
		return 0;
	}
	else if (val >= v_high) {
		return 1;
	}
	else if (val < v_mid) {
		return (val - v_low) / (v_mid - v_low) * 0.5;
	}
	else if (val > v_mid) {
		return (val - v_mid) / (v_high - v_mid) * 0.5 + 0.5;
	}
	return 0.5;
}

var z_ampstats;
function ampstats() {
	if (arguments.length != 4) {
		return;
	}
	if (z_scale === 0) {
		return;
	}
	// list: left_min left_max right_min right_max
	var l_min = Math.abs(arguments[0]);
	var l_max = arguments[1];
	var r_min = Math.abs(arguments[2]);
	var r_max = arguments[3];
	var z = Math.max(l_min, l_max, r_min, r_max);
	z_ampstats = scale_volume(z) * z_scale;
}

function freqpeak() {
	if (arguments.length === 0 || Math.abs(arguments.length % 2) == 1) {
		return;
	}
	if (z_scale === 0) {
		return;
	}
	// each pair of arguments is freq1 amp1 freq2 amp2...
	// first half of list is left audio, last half is right audio
	var point = 0;
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
		if (l_freq < f_low || l_freq > f_high) {
			if (r_freq < f_low || r_freq > f_high) {
				i++;
				continue;
			}
		}
		var x,y,z;
		x = get_pan_position(l_amp, r_amp);
		if (l_amp > r_amp) {
			y = scale_frequency(l_freq);
		}
		else {
			y = scale_frequency(r_freq);
		}
		z = z_ampstats;
		outlet(0, point, x, y, z);
		point++;
		i++;
	}
}

function freqpeak_sort() {

}
