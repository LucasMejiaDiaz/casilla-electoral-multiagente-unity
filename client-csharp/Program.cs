using System.Net;
using System.Text.Json;

const string endpoint = "http://127.0.0.1:5000/get_agents";

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(5)
};

Console.WriteLine($"Consultando {endpoint}...");

try
{
    using HttpResponseMessage response = await httpClient.GetAsync(endpoint);

    if (response.StatusCode != HttpStatusCode.OK)
    {
        Console.Error.WriteLine($"El backend respondió {(int)response.StatusCode} ({response.ReasonPhrase}).");
        return 1;
    }

    string json = await response.Content.ReadAsStringAsync();
    using JsonDocument document = JsonDocument.Parse(json);

    Console.WriteLine(JsonSerializer.Serialize(
        document.RootElement,
        new JsonSerializerOptions { WriteIndented = true }));

    return 0;
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"No se pudo conectar con el backend: {exception.Message}");
    return 1;
}
catch (TaskCanceledException)
{
    Console.Error.WriteLine("La petición agotó el tiempo de espera.");
    return 1;
}
catch (JsonException exception)
{
    Console.Error.WriteLine($"El backend devolvió JSON inválido: {exception.Message}");
    return 1;
}
