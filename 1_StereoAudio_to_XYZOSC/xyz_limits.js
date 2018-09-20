autowatch = 1;
inlets = 1;
outlets = 1;

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
var limits = 0;
function set_limits(val) {
	limits = val;
}
var random = 0;
function set_random(val) {
	random = val;
}
var limits_list = {
	//0: [x,y,z],
	// front, z = 1
	0: [0,0,1],
	1: [0.5,0,1],
	2: [1,0,1],
	3: [0,0.5,1],
	4: [0.5,0.5,1],
	5: [1,0.5,1],
	6: [0,1,1],
	7: [0.5,1,1],
	8: [1,1,1],
	// center, z = 0.5
	9: [0,0,0.5],
	10: [0.5,0,0.5],
	11: [1,0,0.5],
	12: [0,0.5,0.5],
	13: [0.5,0.5,0.5],
	14: [1,0.5,0.5],
	15: [0,1,0.5],
	16: [0.5,1,0.5],
	17: [1,1,0.5],
	// back, z = 0.
	18: [0,0,0],
	19: [0.5,0,0],
	20: [1,0,0],
	21: [0,0.5,0],
	22: [0.5,0.5,0],
	23: [1,0.5,0],
	24: [0,1,0],
	25: [0.5,1,0],
	26: [1,1,0]
};
var next = "limits";

sendpoints.immediate = 1;
function sendpoints() {
	getpoints(false);
}

sendpoints_sort.immediate = 1;
function sendpoints_sort() {
	getpoints(true);
}

getpoints.local = 1;
function getpoints(sort) {
	if (level === 0) {
		return;
	}
	if (peaks === 0) {
		return;
	}
	else if (peaks == 1) {
		sort = false;
	}
	if (limits === 0 && random === 0) {
		return;
	}
	else if (limits == 1 && random === 0) {
		next = "limits";
	}
	else if (random == 1 && limits === 0) {
		next = "random";
	}
	var res = {};
	var res_index = [];
	var id = 0;
	var x, y, z, objOut;
	for(var point = 0; point < peaks; point++) {
		if (next == "limits") {
			this.patcher.getnamed("limits_urn_in").message("bang");
			objOut = this.patcher.getnamed("limits_urn_out");
			var i = objOut.getvalueof();
			x = parseFloat(limits_list[i][0]);
			y = limits_list[i][1];
			z = limits_list[i][2];
			if (random == 1) {
				next = "random";
			}
		}
		else if (next == "random") {
			x = Math.random();
			y = Math.random();			
			z = Math.random();
			if (limits == 1) {
				next = "limits";
			}
		}
		// scale
		this.patcher.getnamed("x_in").message("float", x);
		objOut = this.patcher.getnamed("x_out");
		x = objOut.getvalueof();
		this.patcher.getnamed("y_in").message("float", y);
		objOut = this.patcher.getnamed("y_out");
		y = objOut.getvalueof();
		this.patcher.getnamed("z_in").message("float", z);
		objOut = this.patcher.getnamed("z_out");
		z = objOut.getvalueof();
		if (!sort) {
			outlet(0, point, x, y, z);
		}
		else {
			res[id] = [x,y,z];
			res_index.push([id,z]);
			id++;			
		}
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
	point = 0;
	for(point = 0; point < peaks; point++) {
		if (!res_index[point]) {
			break;
		}
		outlet(0, point, res[res_index[point][0]][0], res[res_index[point][0]][1], res[res_index[point][0]][2]);
	}
}
