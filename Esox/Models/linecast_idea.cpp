#include <iostream>
#include <vector>
#include <string>
#include <sstream>
#include <cmath>
#include <random>

using namespace std;

const double EPS = 1e-9;

class LinearSystemSolver {
    vector<vector<double>> augmented;
    vector<string> latex_steps;
    int n;
    bool is_homogeneous;

    void add_step(const string& desc, const vector<vector<double>>& m) {
        stringstream ss;
        ss << "\\textbf{Шаг " << latex_steps.size()+1 << ":} " << desc << "\n";
        ss << "\\begin{equation*}\n";
        ss << "\\left[\n\\begin{array}{";
        for(int i = 0; i < n+1; ++i) ss << (i < n ? "c" : "|c"); 
        ss << "}\n";
        
        for(const auto& row : m) {
            for(size_t i = 0; i < row.size(); ++i) {
                ss << (abs(row[i]) < EPS ? 0.0 : row[i]) 
                   << (i == row.size()-1 ? " \\\\" : " & ");
            }
            ss << "\n";
        }
        ss << "\\end{array}\n\\right]\n\\end{equation*}\n";
        latex_steps.push_back(ss.str());
    }

public:
    LinearSystemSolver(const vector<vector<double>>& A, const vector<double>& b) 
        : n(A.size()), is_homogeneous(all_of(b.begin(), b.end(), 
          [](double val) { return abs(val) < EPS; })) 
    {
        augmented = A;
        for(size_t i = 0; i < b.size(); ++i) {
            augmented[i].push_back(b[i]);
        }
        add_step("Исходная система", augmented);
    }

    vector<double> solve(string& latex) {
        // Прямой ход
        for(int i = 0; i < n; ++i) {
            // Поиск опорного элемента
            int pivot = i;
            for(int j = i+1; j < n; ++j)
                if(abs(augmented[j][i]) > abs(augmented[pivot][i]))
                    pivot = j;

            if(abs(augmented[pivot][i]) < EPS)
                throw runtime_error("Матрица вырожденная");

            if(pivot != i) {
                swap(augmented[i], augmented[pivot]);
                add_step("Перестановка строк " + to_string(i+1) + 
                       " и " + to_string(pivot+1), augmented);
            }

            // Нормализация
            double div = augmented[i][i];
            for(int j = i; j <= n; ++j) augmented[i][j] /= div;
            add_step("Нормализация строки " + to_string(i+1), augmented);

            // Исключение
            for(int j = i+1; j < n; ++j) {
                double factor = augmented[j][i];
                for(int k = i; k <= n; ++k) {
                    augmented[j][k] -= factor * augmented[i][k];
                }
                add_step("Исключение в строке " + to_string(j+1) + 
                        " используя строку " + to_string(i+1), augmented);
            }
        }

        // Обратный ход
        vector<double> solution(n);
        for(int i = n-1; i >= 0; --i) {
            solution[i] = augmented[i][n];
            for(int j = i+1; j < n; ++j) {
                solution[i] -= augmented[i][j] * solution[j];
            }
        }

        // Формирование вывода
        stringstream final_latex;
        for(const auto& step : latex_steps) final_latex << step << "\n";
        
        final_latex << "\\textbf{Решение:}\n\\begin{equation*}\n";
        final_latex << "\\begin{cases}\n";
        for(int i = 0; i < n; ++i) {
            final_latex << "x_" << i+1 << " = " << solution[i] << " \\\\\n";
        }
        final_latex << "\\end{cases}\n\\end{equation*}";
        
        latex = final_latex.str();
        return solution;
    }
};

vector<vector<double>> generate_invertible_matrix(int n, double det) {
    vector<vector<double>> A(n, vector<double>(n, 0));
    default_random_engine gen;
    uniform_real_distribution<double> dist(-5.0, 5.0);

    // Создаем верхнюю треугольную матрицу
    for(int i = 0; i < n; ++i) {
        for(int j = i; j < n; ++j) {
            A[i][j] = dist(gen);
        }
        if(i == n-1) A[i][i] = det;
        else A[i][i] = 1.0;
    }

    // Добавляем случайные преобразования
    for(int i = 0; i < n; ++i) {
        for(int j = i+1; j < n; ++j) {
            double factor = dist(gen);
            for(int k = 0; k < n; ++k) {
                A[j][k] += factor * A[i][k];
            }
        }
    }
    return A;
}

int main() {
    try {
        int n = 3;
        double det = 6.0;
        
        // Генерация системы
        auto A = generate_invertible_matrix(n, det);
        vector<double> b(n);
        for(auto& val : b) val = rand() % 10 - 5;

        // Решение
        string latex;
        LinearSystemSolver solver(A, b);
        auto solution = solver.solve(latex);

        // Вывод LaTeX
        cout << "\\documentclass{article}\n\\begin{document}\n";
        cout << latex;
        cout << "\n\\end{document}";

    } catch(const exception& e) {
        cerr << "Ошибка: " << e.what() << endl;
    }
    return 0;
}