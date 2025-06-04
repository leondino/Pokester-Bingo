using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    [SerializeField]
    private Transform plate1, plate2; // Reference to the background plates
    [SerializeField]
    private float speed = 1f; // Speed of the background movement
    [SerializeField]
    private float resetPosition = 2000; // X position to reset the background plates

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        plate1.position += new Vector3(-speed, -speed);
        plate2.position += new Vector3(-speed, -speed);

        // Reset positions if they move out of view
        if (plate1.localPosition.x <= -resetPosition)
        {
            plate1.localPosition = new Vector3(resetPosition, resetPosition);
        }
        if (plate2.localPosition.x <= -resetPosition)
        {
            plate2.localPosition = new Vector3(resetPosition, resetPosition);
        }
    }
}
