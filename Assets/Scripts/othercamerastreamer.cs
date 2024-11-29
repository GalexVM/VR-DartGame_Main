using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class CameraStreamClient : MonoBehaviour
{
    private UdpClient udpClient;
    private int port = 12345;  // Puerto del servidor en Python
    private string serverAddress = "192.168.103.99";  // Dirección IP del servidor
    public Camera player2Camera;

    void Start()
    {
        udpClient = new UdpClient();
        udpClient.Connect(serverAddress, port);
    }

    void Update()
    {
        // Obtener la textura de la cámara
        RenderTexture renderTexture = player2Camera.targetTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = null;

        // Convertir la textura a bytes
        byte[] data = texture.EncodeToJPG();

        // Enviar los datos a través de UDP
        udpClient.Send(data, data.Length);
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
