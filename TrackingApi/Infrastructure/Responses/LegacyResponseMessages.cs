namespace TrackingApi.Infrastructure.Responses;

public static class LegacyResponseMessages
{
    public const string LoginSuccess = "User login successfully.";
    public const string TransactionOk = "Transacción OK.";
    public const string TrackingNotFound = "NO EXISTE REGISTRO EN EL SISTEMA SOBRE LA ORDEN TRACKING";
    public const string TrackingAlreadyExists = "YA EXISTE EL REGISTRO EN EL SISTEMA";
    public const string InvalidCredentials = "ERROR EN LOS DATOS ENTREGADOS (username o password incorrectos)";

    public static string MissingFields(IEnumerable<string> fields) =>
        $"ERROR EN LOS DATOS ENTREGADOS ({string.Join(", ", fields)})";
}
