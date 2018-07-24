using UnityEngine;
using System.Collections;

public class _Receive_XYZOSC : MonoBehaviour {
    
   	public OSC osc;
    public GameObject prefab;
    public GameObject groupParent;

	// Use this for initialization
	void Start () {
        osc.SetAllMessageHandler(messageHandler);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void messageHandler(OscMessage message){
        //Debug.Log(message.address);
        makeInstance(message);
    }

    void makeInstance(OscMessage message) {
        float x = message.GetFloat(0);
        float y = message.GetFloat(1);
        float z = message.GetFloat(2);
        // flip z
        z = 1 - z;
        GameObject gameObject = Instantiate(prefab, new Vector3(x,y,z), Quaternion.identity, groupParent.transform);
        Destroy(gameObject,0.1f);
    }

}
