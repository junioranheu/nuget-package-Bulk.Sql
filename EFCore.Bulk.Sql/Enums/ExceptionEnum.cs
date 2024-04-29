using System.ComponentModel;

namespace EFCore.Bulk.Sql.Enums
{
    public enum ExceptionEnum
    {
        [Description("O parâmetro de conexão não deve ser nulo.")]
        ParamConexNaoPodeSerNulo,

        [Description("The connection parameter must not be null.")]
        ParamConexNaoPodeSerNulo_EN,

        [Description("O parâmetro de conexão deve ser do tipo 'Microsoft.Data.SqlClient.SqlConnection', 'System.Data.SqlClient.SqlConnection' ou 'MySqlConnection'.")]
        ParamConexaoDeveSer,

        [Description("The connection parameter must be a 'Microsoft.Data.SqlClient.SqlConnection', System.Data.SqlClient.SqlConnection, or 'MySqlConnection' type.")]
        ParamConexaoDeveSer_EN,

        [Description("Houve um erro interno ao salvar os dados.")]
        ErroInternoSalvar,

        [Description("There was an internal error while saving the data.")]
        ErroInternoSalvar_EN,

        [Description("Houve um erro interno ao mapear a tabela virtual de dados.")]
        ErroInternoMapearTabelaVirtual,

        [Description("There was an internal error while mapping the DataTable.")]
        ErroInternoMapearTabelaVirtual_EN,

        [Description("Houve um erro interno ao atribuir valores à tabela virtual de dados.")]
        ErroInternoAtribuirValoresTabelaVirtual,

        [Description("There was an internal error while assigning values to the DataTable.")]
        ErroInternoAtribuirValoresTabelaVirtual_EN,
        
        [Description("Houve um erro interno ao converter os dados.")]
        ErroInternoConverterDados,

        [Description("There was an internal error while converting the data.")]
        ErroInternoConverterDados_EN,

        [Description("Houve um erro interno ao converter a biblioteca System.Data.SqlClient para Microsoft.Data.SqlClient.")]
        ErroInternoConverterSQLServerDeSystemParaMicrosoft,

        [Description("There was an internal error while converting System.Data.SqlClient to Microsoft.Data.SqlClient.")]
        ErroInternoConverterSQLServerDeSystemParaMicrosoft_EN,

        [Description("Houve um erro interno ao deletar os dados.")]
        ErroInternoDeletar,

        [Description("There was an internal error while deleting the data.")]
        ErroInternoDeletar_EN
    }
}