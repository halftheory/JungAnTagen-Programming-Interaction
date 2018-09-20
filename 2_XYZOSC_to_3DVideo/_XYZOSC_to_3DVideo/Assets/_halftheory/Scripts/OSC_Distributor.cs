using UnityEngine;

namespace _halftheory {
    [RequireComponent(typeof(OSC))]
	public class OSC_Distributor : MonoBehaviour {

		private static bool initialized = false;

		private static bool isInit() {
			if (initialized) {
				return (true);
			}
			if (!MainSettingsVars.initialized) {
				return (false);
			}
			initialized = true;
			return (true);
		}

		void Start() {
			bool test = isInit();
			if (test) {
				//DontDestroyOnLoad(this.gameObject);
				MainSettingsVars.oscComponent.SetAllMessageHandler(messageHandler);
			}
		}

		void messageHandler(OscMessage message) {
			if (message.address.IndexOf("/group") == 0) {
				MainSettingsVars.currentAnimationComponent.collectPoints(message.address, message.GetFloat(0), message.GetFloat(1), message.GetFloat(2));
	            //Debug.Log("HALFTHEORY: "+this.GetType()+": "+message.address+" - found");
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
            Debug.Log("HALFTHEORY: "+this.GetType()+": no function found");
		}
	}
}