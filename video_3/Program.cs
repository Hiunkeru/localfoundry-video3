using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

try
{
    // Conectar directamente al servicio Foundry local (igual que en Python)
    var endpoint = new Uri("http://127.0.0.1:59701/v1/");
    Console.WriteLine($"Conectado a Foundry en: {endpoint}");

    var client = new ChatClient(
        model: "phi-3.5-mini-instruct-trtrtx-gpu:1",
        credential: new ApiKeyCredential("local"), // La clave es dummy en local
        options: new OpenAIClientOptions { Endpoint = endpoint } // ¡Aquí está la conexión!
    );

    // 1. Definir la herramienta (Tool Definition)
    var weatherTool = ChatTool.CreateFunctionTool(
        functionName: "get_weather",
        functionDescription: "Obtiene el clima actual de una ciudad.",
        functionParameters: BinaryData.FromObjectAsJson(new
        {
            type = "object",
            properties = new { location = new { type = "string" } },
            required = new[] { "location" }
        })
    );

    var currencyTool = ChatTool.CreateFunctionTool(
        functionName: "get_currency",
        functionDescription: "Obtiene la tasa de cambio actual de una moneda y la moneda en esa localidad.",
        functionParameters: BinaryData.FromObjectAsJson(new
        {
            type = "object",
            properties = new { location = new { type = "string" } },
            required = new[] { "location" }
        })
    );

    // 2. Chat Loop
    var messages = new List<ChatMessage> { new SystemChatMessage("Eres un experto diciendo información de una localidad") };
    messages.Add(new UserChatMessage("Que moneda necesito para viajar a Japón y cuál es la tasa de cambio actual?"));

    ChatCompletionOptions options = new() { Tools = { weatherTool, currencyTool } };

    // 3. Inferencia
    var response = await client.CompleteChatAsync(messages, options);

    // 4. Interceptar la solicitud de herramienta
    if (response.Value.ToolCalls.Count > 0)
    {
        foreach (var toolCall in response.Value.ToolCalls)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($": Ejecutar '{toolCall.FunctionName}'");
            Console.WriteLine($": {toolCall.FunctionArguments}");
            Console.ResetColor();
            // Aquí tu código real llamaría a la API del tiempo
        }
    }
    else
    {
        Console.WriteLine($"[IA]: {response.Value.Content[0].Text}");
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: {ex.Message}");
    Console.ResetColor();
    Console.WriteLine("\nPosibles soluciones:");
    Console.WriteLine("1. Verifica que el servicio Foundry esté corriendo: foundry service status");
    Console.WriteLine("2. Actualiza la versión del paquete Microsoft.AI.Foundry.Local");
    Console.WriteLine("3. Intenta con otro modelo disponible");
}