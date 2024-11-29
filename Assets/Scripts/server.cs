using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
using System.Threading.Tasks;

public class SensorDataReceiver : MonoBehaviour
{
    public int portSensor = 5000;
    public int portKeyboard = 5001;

    private Thread sensorThread;
    private Thread keyboardThread;
    private bool running = true;

    private CancellationTokenSource cts; // Token de cancelación

    // Referencia al componente TextMeshPro para actualizar el texto
    public TextMeshProUGUI data_text;

    // Lista de luces que se moverán
    public List<Light> directionalLights;
    public List<GameObject> cylinders;
    public GameObject oculusInteractionSamplerRig;

    // Variables para almacenar los valores de velocidad y rotación angular
    private float VX, VY, VZ, AX, AY, AZ;

    private void Start()
    {
        cts = new CancellationTokenSource(); // Inicializar el token de cancelación

        sensorThread = new Thread(() => StartListening(portSensor, HandleSensorData, true, cts.Token));
        sensorThread.Start();

        keyboardThread = new Thread(() => StartListening(portKeyboard, HandleKeyboardData, false, cts.Token));
        keyboardThread.Start();
    }

    private void StartListening(int port, Action<string> dataHandler, bool useAsync, CancellationToken cancellationToken)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Debug.Log($"Listening on port {port}...");

        while (running && !cancellationToken.IsCancellationRequested)
        {
            if (listener.Pending())
            {
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    Debug.Log($"Cliente conectado en puerto {port}.");

                    if (useAsync)
                    {
                        ReadSensorDataAsync(reader, dataHandler, cancellationToken).Wait(); // Llamada asincrónica
                    }
                    else
                    {
                        ReadKeyboardData(reader, dataHandler, cancellationToken); // Llamada sincrónica usando ReadLine
                    }
                }
            }
        }

        listener.Stop(); // Detener el listener al finalizar
        Debug.Log($"Listener on port {port} stopped.");
    }

    private async Task ReadSensorDataAsync(StreamReader reader, Action<string> dataHandler, CancellationToken cancellationToken)
    {
        char[] buffer = new char[256];
        while (running && !cancellationToken.IsCancellationRequested)
        {
            int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                string receivedData = new string(buffer, 0, bytesRead);
                Debug.Log($"Datos recibidos en puerto 5000 (Sensor): {receivedData}");
                dataHandler(receivedData);
            }
        }
    }

    private void ReadKeyboardData(StreamReader reader, Action<string> dataHandler, CancellationToken cancellationToken)
    {
        string receivedData;
        while (running && !cancellationToken.IsCancellationRequested && (receivedData = reader.ReadLine()) != null)
        {
            Debug.Log($"Datos recibidos en puerto 5001 (Teclado): {receivedData}");
            dataHandler(receivedData);
        }
    }

    private void HandleSensorData(string data)
    {
        Debug.Log($"Sensor data received: {data}");
        try
        {
            ProcessSensorData(data);
        }
        catch (Exception e)
        {
            Debug.LogError("Error al leer los datos del sensor: " + e.Message);
        }
    }

    private void HandleKeyboardData(string data)
    {
        Debug.Log($"Keyboard data received: {data}");
        try
        {
            moveObjects(data);
        }
        catch (Exception e)
        {
            Debug.LogError("Error al leer los datos del teclado: " + e.Message);
        }
    }

    private int currentLightIndex = 0;
    private int currentCylinderIndex = 0; 
    private bool controllingLights = true;





    private void moveObjects(string jsonData)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            // Verifica si se presionó la tecla "U" para hacer TP a la primera luz o cilindro
            if (jsonData[3] == 'U' || jsonData[3] == 'u')
            {
                // Cambiar el control entre luces y cilindros
                controllingLights = !controllingLights;  // Alternar entre luces y cilindros

                if (controllingLights)
                {
                    // Asegúrate de que haya luces en la lista
                    if (directionalLights != null && directionalLights.Count > 0)
                    {
                        // Teletransportar la primera luz a la posición (0, 3, 0)
                        directionalLights[0].transform.position = new Vector3(0, 3, 0);
                        Debug.Log($"Teletransportando la luz a (0, 3, 0)");
                    }
                    else
                    {
                        Debug.LogWarning("No se ha asignado ninguna luz a la lista directionalLights.");
                    }
                }
                else
                {
                    // Asegúrate de que haya cilindros en la lista
                    if (cylinders != null && cylinders.Count > 0)
                    {
                        // Teletransportar el primer cilindro a la posición (0, 3, 0)
                        cylinders[0].transform.position = new Vector3(0, 3, 0);
                        Debug.Log($"Teletransportando el cilindro a (0, 3, 0)");
                    }
                    else
                    {
                        Debug.LogWarning("No se ha asignado ningún cilindro a la lista cylinders.");
                    }
                }
                return;
            }


            // Resto del código para mover objetos
            float moveAmount = 1f;
            float moveAmount1 = 45f;

            if (controllingLights)
            {
                if (directionalLights == null || directionalLights.Count == 0)
                {
                    Debug.LogWarning("No se ha asignado ninguna luz a la lista directionalLights.");
                    return;
                }

                Light currentLight = directionalLights[currentLightIndex];
                ProcessMovement(jsonData, currentLight.transform);

                if (jsonData[3] == 'O' || jsonData[3] == 'o')
                {
                    // Cambiar al siguiente objeto
                    currentLightIndex = (currentLightIndex + 1) % directionalLights.Count;
                    Light newLight = directionalLights[currentLightIndex];

                    // Teletransportar el nuevo objeto a (0, 3, 0)
                    newLight.transform.position = new Vector3(0, 3, 0);
                    Debug.Log($"Cambiando a la siguiente luz: {currentLightIndex} y teletransportando a (0, 3, 0)");
                }
            }
            else
            {
                if (cylinders == null || cylinders.Count == 0)
                {
                    Debug.LogWarning("No se ha asignado ningún cilindro a la lista cylinders.");
                    return;
                }

                GameObject currentCylinder = cylinders[currentCylinderIndex];
                ProcessMovement(jsonData, currentCylinder.transform);

                if (jsonData[3] == 'O' || jsonData[3] == 'o')
                {
                    // Cambiar al siguiente objeto
                    currentCylinderIndex = (currentCylinderIndex + 1) % cylinders.Count;
                    GameObject newCylinder = cylinders[currentCylinderIndex];

                    // Teletransportar el nuevo objeto a (0, 3, 0)
                    newCylinder.transform.position = new Vector3(0, 3, 0);
                    Debug.Log($"Cambiando al siguiente cilindro: {currentCylinderIndex} y teletransportando a (0, 3, 0)");
                }
            }
        });
    }


    private void ProcessMovement(string jsonData, Transform transform)
    {
        float moveAmount = 1f;
        float moveAmount1 = 45f;

        if (jsonData[3] == 'W' || jsonData[3] == 'w')
            transform.position += Vector3.forward * moveAmount;

        if (jsonData[3] == 'A' || jsonData[3] == 'a')
            transform.position += Vector3.left * moveAmount;

        if (jsonData[3] == 'S' || jsonData[3] == 's')
            transform.position += Vector3.back * moveAmount;

        if (jsonData[3] == 'D' || jsonData[3] == 'd')
            transform.position += Vector3.right * moveAmount;

        if (jsonData[3] == 'T' || jsonData[3] == 't')
            transform.position += Vector3.up * moveAmount;

        if (jsonData[3] == 'G' || jsonData[3] == 'g')
            transform.position += Vector3.down * moveAmount;

        if (jsonData[3] == 'N' || jsonData[3] == 'n')
            transform.Rotate(Vector3.right * moveAmount1);

        if (jsonData[3] == 'M' || jsonData[3] == 'm')
            transform.Rotate(Vector3.up * moveAmount1);

        if (jsonData[3] == 'E' || jsonData[3] == 'e')
            oculusInteractionSamplerRig.transform.position = new Vector3(20, 20, 20);

    }















    private void ProcessSensorData(string jsonData)
    {
        Debug.Log("Procesando datos del sensor...");
        try
        {
            var sensorData = JsonUtility.FromJson<SensorData>(jsonData);
            Debug.Log($"VelocidadX: {sensorData.VelocidadX}, VelocidadY: {sensorData.VelocidadY}, VelocidadZ: {sensorData.VelocidadZ}, AngularX: {sensorData.AngularX}, AngularY: {sensorData.AngularY}, AngularZ: {sensorData.AngularZ}");

            VX = sensorData.VelocidadZ;
            VY = sensorData.VelocidadY;
            VZ = sensorData.VelocidadX;
            AX = sensorData.AngularX;
            AY = sensorData.AngularY;
            AZ = sensorData.AngularZ;

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                WriteDataToFile();
                UpdateUI();
            });
        }
        catch (Exception e)
        {
            Debug.LogError("Error al procesar los datos del sensor: " + e.Message);
        }
    }

    private void WriteDataToFile()
    {
        Debug.Log("Escribiendo datos en el archivo...");
        string filePath = Path.Combine(Application.persistentDataPath, "SensorData.txt");

        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine($"{VX:F2}");
            writer.WriteLine($"{VY:F2}");
            writer.WriteLine($"{VZ:F2}");
            writer.WriteLine($"{AX:F2}");
            writer.WriteLine($"{AY:F2}");
            writer.WriteLine($"{AZ:F2}");
        }
        Debug.Log($"Datos escritos en el archivo: {filePath}");
    }

    private void UpdateUI()
    {
        if (data_text != null)
        {
            data_text.fontSize = 24f;
            data_text.text = $"VX: {VX:F2}\n" +
                             $"VY: {VY:F2}\n" +
                             $"VZ: {VZ:F2}\n" +
                             $"AX: {AX:F2}\n" +
                             $"AY: {AY:F2}\n" +
                             $"AZ: {AZ:F2}";
        }
        else
        {
            Debug.LogWarning("Referencia a data_text no está asignada.");
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        cts.Cancel(); // Cancelar las operaciones asíncronas
        sensorThread?.Join();
        keyboardThread?.Join();
        Debug.Log("Aplicación cerrada y todos los hilos detenidos.");
    }
}

[Serializable]
public class SensorData
{
    public float VelocidadX;
    public float VelocidadY;
    public float VelocidadZ;
    public float AngularX;
    public float AngularY;
    public float AngularZ;
}
