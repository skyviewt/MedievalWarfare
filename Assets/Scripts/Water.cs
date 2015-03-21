
using UnityEngine;
using System.Collections;

[System.Serializable]
public class Water : MonoBehaviour {

	
	// UV speed
	public float m_SpeedU = 0.1f;
	public float m_SpeedV = -0.1f;

	
	// Update is called once per frame
	void Update () {

		// Update new UV speed
		float newOffsetU = Time.time * m_SpeedU;
		float newOffsetV = Time.time * m_SpeedV;
		
		// Check if there is renderer component
		if (this.renderer)
		{
			// Update main texture offset
			renderer.material.mainTextureOffset = new Vector2(newOffsetU, newOffsetV);
		}
	}

}