using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;

public class DisplayIPAddress : MonoBehaviour
{
    // Referencia al componente TextMeshProUGUI para mostrar la IP
    public TextMeshProUGUI ipAddressText;

    private void Start()
    {
        // Obtener la dirección IPv4 y actualizar el texto
        string ipv4Address = GetLocalIPv4Address();
        UpdateIPText(ipv4Address);
    }

    private string GetLocalIPv4Address()
    {
        // Obtiene la dirección IP de la red actual
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

        return ip != null ? ip.ToString() : "No IP found";
    }

    private void UpdateIPText(string ipAddress)
    {
        if (ipAddressText != null)
        {
            ipAddressText.text = "IPv4: " + ipAddress;
        }
        else
        {
            Debug.LogWarning("Referencia a ipAddressText no está asignada.");
        }
    }
}
