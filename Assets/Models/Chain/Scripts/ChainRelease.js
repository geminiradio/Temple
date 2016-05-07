#pragma strict

var Keystroke = "";

function Start () {
	
}

function Update () {
	if(Input.GetKeyUp(Keystroke)){
		if(gameObject.GetComponent.<HingeJoint>()){
			Destroy(gameObject.GetComponent.<HingeJoint>());			
		}
		
		if(gameObject.GetComponent(CharacterJoint)){
			Destroy(gameObject.GetComponent(CharacterJoint));
			
		}
	}
}