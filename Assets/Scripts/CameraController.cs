using UnityEngine;
using System.Collections;
public class CameraController: MonoBehaviour
{
	public float speed = 0.3f;
	void Start(){
		transform.position = new Vector3 (1, 3, 0);
		transform.rotation = Quaternion.Euler (60, 270, 0);
	}
	void Update ()
	{
		float lr = Input.GetAxis("Vertical") * speed; // world z axis
		float ud = Input.GetAxis("Horizontal") * speed; // world x axis
		transform.Translate (0, 0, ud, Space.World);
		transform.Translate (-lr, 0, 0, Space.World);
	}
} 