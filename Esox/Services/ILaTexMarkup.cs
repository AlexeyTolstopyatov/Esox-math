using System.Runtime.InteropServices;
using System.Text;

namespace Esox.Services;

/// <summary>
/// ILaTexMarkup представляет не полную
/// поддержку макросов LaTex, а лишь
/// использует некоторые
/// элементы окружения для создания разметки.
/// </summary>
public interface ILaTexMarkup
{
    /// <summary>
    /// Возвращает случайную заглавную букву
    /// латинского алфавита в качестве
    /// наименования объекта
    /// </summary>
    /// <returns></returns>
    string MakeName();

    /// <summary>
    /// Переводит расширенную матрицу
    /// системы линейных уравнений
    /// из матричного вида в вид объединения.
    /// </summary>
    /// <returns></returns>
    string MakeCases(double[,] common, double[] freed, [Optional] string otherName);

    /// <summary>
    /// Переводит систему линейных уравнений
    /// в вид расширенной матрицы
    /// </summary>
    /// <returns></returns>
    string MakePMatrix(double[,] common, double[] freed, [Optional] string otherName);

    /// <summary>
    /// Создает систему линейных уравнений
    /// с подписью \text{} (описанием решения)
    /// </summary>
    /// <param name="text">Описание</param>
    /// <param name="isBold">Сделать жирным?</param>
    /// <param name="extended">Расширенная матрица</param>
    /// <param name="otherName">Другое наименование объекта</param>
    /// <returns></returns>
    string MakeExtendedPMatrixWithText(string text, bool isBold, double[,] extended, [Optional] string otherName);
    
}