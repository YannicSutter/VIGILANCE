using UnityEngine;

public class ClientDriver : MonoBehaviour
{
    InputFrame input = new InputFrame();

    // UNITY METHODS
    private void FixedUpdate()
    {
       Server.Instance.Tick(input, Time.fixedDeltaTime);
    }
}
