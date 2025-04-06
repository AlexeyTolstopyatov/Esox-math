using System.Runtime.InteropServices;

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
    string MakeCases(int[,] common, int[] freed, [Optional] string otherName);

    /// <summary>
    /// Переводит систему линейных уравнений
    /// в вид расширенной матрицы
    /// </summary>
    /// <returns></returns>
    string MakePMatrix(int[,] common, int[] freed, [Optional] string otherName);

    /// <summary>
    /// Создает систему линейных уравнений
    /// с подписью \text{} (описанием решения)
    /// </summary>
    /// <param name="text">Описание</param>
    /// <param name="isBold">Сделать жирным?</param>
    /// <returns></returns>
    string MakePMatrixWithText(string text, bool isBold);
    
}