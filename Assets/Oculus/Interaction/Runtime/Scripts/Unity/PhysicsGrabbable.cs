using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Oculus.Interaction
{
    public class PhysicsGrabbable : MonoBehaviour
    {
        [SerializeField]
        private Grabbable _grabbable;

        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        [Tooltip("If enabled, the object's mass will scale appropriately as the scale of the object changes.")]
        private bool _scaleMassWithSize = true;

        private bool _savedIsKinematicState = false;
        private bool _isBeingTransformed = false;
        private Vector3 _initialScale;
        private bool _hasPendingForce;

        private Vector3 _defaultLinearVelocity = new Vector3(0f, 0f, 0f);
        private Vector3 _defaultAngularVelocity = new Vector3(0f, 0f, 0f);

        private Vector3 _linearVelocity;
        private Vector3 _angularVelocity;

        protected bool _started = false;

 

        public event Action<Vector3, Vector3> WhenVelocitiesApplied = delegate { };

        private void Reset()
        {
            _grabbable = this.GetComponent<Grabbable>();
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            this.AssertField(_grabbable, nameof(_grabbable));
            this.AssertField(_rigidbody, nameof(_rigidbody));
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                _grabbable.WhenPointerEventRaised += HandlePointerEventRaised;
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                _grabbable.WhenPointerEventRaised -= HandlePointerEventRaised;
            }
        }

        private void HandlePointerEventRaised(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Select:
                    if (_grabbable.SelectingPointsCount == 1 && !_isBeingTransformed)
                    {
                        DisablePhysics();
                    }
                    break;
                case PointerEventType.Unselect:
                    if (_grabbable.SelectingPointsCount == 0)
                    {
                        ReenablePhysics();
                    }
                    break;
            }
        }

        private void DisablePhysics()
        {
            _isBeingTransformed = true;
            CachePhysicsState();
            _rigidbody.isKinematic = true;
        }

        private void ReenablePhysics()
        {
            _isBeingTransformed = false;
            if (_scaleMassWithSize)
            {
                float initialScaledVolume = _initialScale.x * _initialScale.y * _initialScale.z;

                Vector3 currentScale = _rigidbody.transform.localScale;
                float currentScaledVolume = currentScale.x * currentScale.y * currentScale.z;

                float changeInMassFactor = currentScaledVolume / initialScaledVolume;
                _rigidbody.mass *= changeInMassFactor;
            }

            _rigidbody.isKinematic = _savedIsKinematicState;
        }

        public void ApplyVelocities(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            ReadDataFromFile();

            Debug.Log("Throwing object from position: " + _rigidbody.position);
            Debug.Log($"Default Linear Velocity: {_defaultLinearVelocity}");
            Debug.Log($"Default Angular Velocity: {_defaultAngularVelocity}");

            _hasPendingForce = true;
            _linearVelocity = _defaultLinearVelocity;
            _angularVelocity = _defaultAngularVelocity;

           

            // Enviar datos al servidor TCP
            SendDataToServer(_rigidbody.position, _linearVelocity, _angularVelocity);
        }

        private void SendDataToServer(Vector3 position, Vector3 linearVelocity, Vector3 angularVelocity)
        {
            string serverIP = "192.168.3.64";
            int serverPort = 5300;

            // Crear un hilo para enviar datos sin bloquear el flujo principal.
            System.Threading.Thread sendThread = new System.Threading.Thread(() =>
            {
                try
                {
                    using (TcpClient client = new TcpClient(serverIP, serverPort))
                    using (NetworkStream stream = client.GetStream())
                    {
                        //Nombre del objeto
                        string objectName = gameObject.name;
                        string dataToSend = $"Id:({objectName}), Position: {position}, LinearVelocity: {linearVelocity}, AngularVelocity: {angularVelocity}";
                        byte[] data = Encoding.UTF8.GetBytes(dataToSend);

                        stream.Write(data, 0, data.Length);
                        Debug.Log("Data sent to server: " + dataToSend);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error sending data to server: " + e.Message);
                }
            });

            // Iniciar el hilo.
            sendThread.IsBackground = true; // Configurar como hilo de fondo para que no bloquee la salida de la aplicación.
            sendThread.Start();
        }


        private void ReadDataFromFile()
        {
            string filePath = Path.Combine(Application.persistentDataPath, "SensorData.txt");
            try
            {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    if (lines.Length >= 6)
                    {
                        _defaultLinearVelocity = new Vector3(float.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]));
                        _defaultAngularVelocity = new Vector3(float.Parse(lines[3]), float.Parse(lines[4]), float.Parse(lines[5]));
                    }

                    File.WriteAllText(filePath, "");
                }
                else
                {
                    Debug.LogWarning($"El archivo no existe en la ruta: {filePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error al leer el archivo: " + e.Message);
            }
        }

        private void FixedUpdate()
        {
            if (_hasPendingForce)
            {
                _hasPendingForce = false;
                _rigidbody.AddForce(_linearVelocity, ForceMode.VelocityChange);
                _rigidbody.AddTorque(_angularVelocity, ForceMode.VelocityChange);
                WhenVelocitiesApplied(_linearVelocity, _angularVelocity);
            }
        }

        private void CachePhysicsState()
        {
            _savedIsKinematicState = _rigidbody.isKinematic;
            _initialScale = _rigidbody.transform.localScale;
        }

        #region Inject

        public void InjectAllPhysicsGrabbable(Grabbable grabbable, Rigidbody rigidbody)
        {
            InjectGrabbable(grabbable);
            InjectRigidbody(rigidbody);
        }

        public void InjectGrabbable(Grabbable grabbable)
        {
            _grabbable = grabbable;
        }

        public void InjectRigidbody(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
        }

        public void InjectOptionalScaleMassWithSize(bool scaleMassWithSize)
        {
            _scaleMassWithSize = scaleMassWithSize;
        }

        #endregion
    }
}
