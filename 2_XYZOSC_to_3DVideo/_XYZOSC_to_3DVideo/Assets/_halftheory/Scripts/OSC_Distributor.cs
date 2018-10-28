using UnityEngine;
using System;
using System.Collections;

namespace _halftheory {
    [RequireComponent(typeof(OSC))]
	public class OSC_Distributor : MonoBehaviour {

		IEnumerator Start() {
            yield return StartCoroutine(Initialize());
		}           
		IEnumerator Initialize() {
			while(!MainSettingsVars.initialized) {
			     yield return null;
			}
			MainSettingsVars.oscComponent.SetAllMessageHandler(messageHandler);
			yield break;
		}

		private char[] splitter = {'/'};
		string[] parts;
		int point = 0;
		int group = 0;

		void messageHandler(OscMessage message) {
			if (MainSettingsVars.currentAnimationObject == null) {
				return;
			}
			if (message.address.IndexOf("/point/") == 0 && message.values.Count == 3) {
				parts = message.address.Split(splitter, System.StringSplitOptions.RemoveEmptyEntries);
				point = int.Parse(parts[1]);
				// restrict number of points
				if (point >= MainSettingsVars.pointsLength) {
					return;
				}
				// collect points into dictionary
				MainSettingsVars.currentAnimationComponent.collectPoints(point, message.GetFloat(0), message.GetFloat(1), message.GetFloat(2));
				return;
			}
			else if (message.address.IndexOf("/group/") == 0) {
				parts = message.address.Split(splitter, System.StringSplitOptions.RemoveEmptyEntries);
				group = int.Parse(parts[1]);
				switch (parts[2]) {
				    case "active":
						MainSettingsVars.currentAnimationComponent.meshComponents[group].active = message.GetBool(0);
				    	break;
				    case "peaks":
						MainSettingsVars.currentAnimationComponent.meshComponents[group].peaks = message.GetInt(0);
				    	break;
				    case "level":
						MainSettingsVars.currentAnimationComponent.meshComponents[group].level = message.GetFloat(0);
				    	break;
				}
				return;
			}
			else if (message.address == "/fps") {
				MainSettingsVars.currentAnimationComponent.data.fps = message.GetInt(0);
				return;
			}
			else if (message.address == "/x_low") {
				MainSettingsVars.currentAnimationComponent.data.x_low = message.GetFloat(0);
				return;
			}
			else if (message.address == "/x_high") {
				MainSettingsVars.currentAnimationComponent.data.x_high = message.GetFloat(0);
				return;
			}
			else if (message.address == "/y_low") {
				MainSettingsVars.currentAnimationComponent.data.y_low = message.GetFloat(0);
				return;
			}
			else if (message.address == "/y_high") {
				MainSettingsVars.currentAnimationComponent.data.y_high = message.GetFloat(0);
				return;
			}
			else if (message.address == "/z_low") {
				MainSettingsVars.currentAnimationComponent.data.z_low = message.GetFloat(0);
				return;
			}
			else if (message.address == "/z_high") {
				MainSettingsVars.currentAnimationComponent.data.z_high = message.GetFloat(0);
				return;
			}
			else if (message.address == "/audio_file_path") {
				MainSettingsVars.currentAnimationComponent.data.audio_file_path = message.GetString();
				return;
			}
			else if (message.address == "/active") {
				MainSettingsVars.currentAnimationComponent.active = message.GetBool(0);
				return;
			}
            Debug.Log("HALFTHEORY: "+this.GetType()+": no function found for "+message.address);
		}
	}
}