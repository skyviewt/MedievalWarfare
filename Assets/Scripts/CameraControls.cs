using UnityEngine;
using System.Collections;

public class CameraControls : MonoBehaviour
{
	public float speed = 0.3f;

	void Start(){
		transform.position = new Vector3 (0, 5, -3);
		transform.rotation = Quaternion.Euler (45, 0, 0);
	}

	void Update ()
	{
		float ud = Input.GetAxis("Vertical") * speed; // world z axis
		float lr = Input.GetAxis("Horizontal") * speed; // world x axis
		transform.Translate (0, 0, ud, Space.World);
		transform.Translate (lr, 0, 0, Space.World);
	}
} 