autowatch = 1;
inlets = 1;
outlets = 2;

var freq_high, freq_mid, freq_low, thresh, addlast;
function frequency_high(val) {
	freq_high = val;
}
function frequency_mid(val) {
	freq_mid = val;
}
function frequency_low(val) {
	freq_low = val;
}
function threshold(val) {
	thresh = val;
}
var dim = 32;
function matrix_dim(val) {
	dim = val;
	addlast = parseFloat(1 / dim / 2);
}
var poly_voices = 32;
function voices(val) {
	poly_voices = val;
}

list.immediate = 1;
function list() {
	var target = 1;
	var amp, freq, pan, objOut, tmp;
	for(var i = 0; i < arguments.length; i++) {
		// above threshold?
		if (thresh > arguments[i]) {
			continue;
		}
		// amp
		this.patcher.getnamed("amp_in").message("float", arguments[i]);
    	objOut = this.patcher.getnamed("amp_out");
		amp = objOut.getvalueof();
		// get x (pan) y (freq)
		tmp = parseFloat(i / dim);
		// pan - after decimal of tmp
		pan = tmp % 1 + addlast;
		this.patcher.getnamed("pan_in").message("float", pan);
    	objOut = this.patcher.getnamed("pan_out");
		pan = objOut.getvalueof();
		// freq - int before decimal of tmp
		freq = Math.floor(tmp) / dim + addlast;
		this.patcher.getnamed("frequency_in").message("float", freq);
    	objOut = this.patcher.getnamed("frequency_out");
		freq = objOut.getvalueof();

		if (target > poly_voices) {
			target = 1;
		}
		outlet(1, target); // target
		target++;
		outlet(0, amp, freq, pan); // amp - freq - pan (-1. 1.)
	}
}
