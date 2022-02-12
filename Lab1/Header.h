#pragma once
#include <iostream>
class Ratio {
    int Numerator, Denominator;
public:

    Ratio() {
        Numerator = 0;
        Denominator = 1;
    };
    Ratio(int num, int denom) {
        if (denom == 0) {
            Numerator = 0;
            Denominator = 1;
            std::cout << "Знаменатель не может быть равен нулю\n";
            return;
        }
        Numerator = num * (copysign(1, denom));
        Denominator = denom * (copysign(1, denom));
    }

    int GetNumerator() {
        return Numerator;
    }
    void SetNumerator(int num) {
        Numerator = num * (copysign(1, num));
    }

    int GetDenominator() {
        return Denominator;
    }
    void SetDenominator(int denom) {
        if (denom == 0) {
            Denominator = 1;
            std::cout << "Знаменатель не может быть равен нулю\n";
            return;
        }
        Denominator = denom * (copysign(1, denom));
        Numerator *= (copysign(1, denom));
    }

    void ConsoleInput() {
        int num, denom;
        std::cout << "Enter numerator and denominator:";
        std::cin >> num>> denom;
  
        if (denom == 0) {
            std::cout << "Знаменатель не может быть равен нулю\n";
            return;
        }
        Numerator = num * (copysign(1, denom));
        Denominator = denom * (copysign(1, denom));
    }
    void ConsoleOutput() {
        std::cout << Numerator << "/" << Denominator<<"\n";
    }

    float GetValue() {
        return  (float)Numerator / (float)Denominator;
    }
   
    int gcd(int a, int b) {
        return b ? gcd(b, a % b) : a;
    }

    void Reduce() {
        int delit = gcd(Numerator, Denominator);
        Numerator /= delit;
        Denominator /= delit;
    }
    void Raise(int power) {
        Numerator = pow(Numerator, power);
        Denominator = pow(Denominator, power);
    }
};
#pragma region Опреторы_сравнения

bool operator == (Ratio a, Ratio b) {
    return a.GetValue() == b.GetValue();
}
bool operator != (Ratio a, Ratio b) {
    return a.GetValue() != b.GetValue();
}
bool operator > (Ratio a, Ratio b) {
    return a.GetValue() > b.GetValue();
}
bool operator < (Ratio a, Ratio b) {
    return a.GetValue() < b.GetValue();
}
bool operator >= (Ratio a, Ratio b) {
    return a.GetValue() >= b.GetValue();
}
bool operator <= (Ratio a, Ratio b) {
    return a.GetValue() <= b.GetValue();
}
#pragma endregion

#pragma region Операторы_преобразование

Ratio operator + (Ratio a, Ratio b) {
    int num, denom, numA = a.GetNumerator(), denomA = a.GetDenominator(), numB = b.GetNumerator(), denomB = b.GetDenominator();
    num = numA * denomB + numB * denomA;
    denom = denomA * denomB;
    return Ratio(num, denom);
}
Ratio operator - (Ratio a, Ratio b)
{
    int num, denom, numA = a.GetNumerator(), denomA = a.GetDenominator(), numB = b.GetNumerator(), denomB = b.GetDenominator();
    num = numA * denomB - numB * denomA;
    denom = denomA * denomB;
    return Ratio(num, denom);
}
Ratio operator * (Ratio a, Ratio b)
{
    int num, denom, numA = a.GetNumerator(), denomA = a.GetDenominator(), numB = b.GetNumerator(), denomB = b.GetDenominator();
    num = numA * numB;
    denom = denomA * denomB;
    return Ratio(num, denom);
}
Ratio operator / (Ratio a, Ratio b)
{
    int num, denom, numA = a.GetNumerator(), denomA = a.GetDenominator(), numB = b.GetNumerator(), denomB = b.GetDenominator();
    num = numA * denomB;
    denom = denomA * numB;
    return Ratio(num, denom);
}
#pragma endregion
