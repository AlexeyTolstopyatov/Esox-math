# Esox-math
Esox генерирует и решает системы линейных алгебраических
уравнений любого порядка (от 2 до 20). Поддерживает
системные цвета, если операционная система - Windows 10 и выше.

### Снимки экрана
Акцент и темная тема:

<img src="Screenshots/dark.png" height="300" width="400">

Акцент и светлая тема

<img src="Screenshots/white.png" height="300" width="400">

### [Описание алгоритмов](INSIDE.md)
Постепенно будет описываться в [здесь](INSIDE.md), или переедет в новый каталог
с численными методами.

### Режимы работы
Приложение способно как создавать и решать системы линейных уравнений, так и решать уже установленную расширенную
матрицу системы. С обновления `1.2.0.0` добавлены два
раздела параметров для новой матрицы системы и для существующей
матрицы системы.

<img src="Screenshots/fcases.png" height="300" width="500">

На снимке экрана выше представлено составление фундаментальной
совокупности решений для новой (сгенерированной) матрицы
коэффициентов.

<img src="Screenshots/students update.png" height="300" width="400">

На данном снимке экрана представлен ход решения для
существующей матрицы системы (заданной вручную, используя `LaTeX` окружения).

> На момент обновления `README` использование комманды `\pmatrix`
при десериализации не включено, поэтому матрицу системы следует задавать
явными блоками окружения `\begin{...}` и `\end{...}`

# Используется
 - [WpfMath](https://github.com/nevgeny/wpf-math) - Элемент управления окна для WPF приложений, который рисует разметку `LaTeX`
 - [HandyControl](https://github.com/HandyOrg/HandyControl) - Огромная библиотека элементов управления
 и расширение возможностей для использования MVVM принципов.
 - .NET 6.0,
 - C# 10

### Источники, найденные во время изучения вопроса, выложены в репозитории или будут выложены в репозитории позже. 
