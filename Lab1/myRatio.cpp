#include "myRatio.h"
#include <iostream>

class myRatio {
private:
    int Numerator;
    unsigned int Denominator;

    int GetDivider(int a, int b) {
        return b ? GetDivider(b, a % b) : a;
    }
public:
    //>>
    myRatio() {
        Numerator = 0;
        Denominator = 1;
    };
    myRatio(int num, int denom) {
        if (denom == 0) {
            Numerator = 0;
            Denominator = 1;
            std::cout << "Знаменатель не может быть равен нулю\n";
            return;
        }
        Numerator = num * (copysign(1, denom));
        Denominator = denom * (copysign(1, denom));
        Reduce();
    }

    int GetNumerator() {
        return Numerator;
    }
    void SetNumerator(int num) {
        Numerator = num * (copysign(1, num));
        Reduce();
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
        Reduce();
    }

    void ConsoleInput() {
        int num, denom;
        std::cout << "Enter numerator and denominator:";
        std::cin >> num >> denom;

        if (denom == 0) {
            std::cout << "Знаменатель не может быть равен нулю\n";
            return;
        }
        Numerator = num * (copysign(1, denom));
        Denominator = denom * (copysign(1, denom)); \
            Reduce();
    }
    void ConsoleOutput() {
        Reduce();
        std::cout << Numerator << "/" << Denominator << "\n";
    }

    float GetValue() {
        return  (float)Numerator / (float)Denominator;
    }

    void Reduce() {
        int delit = GetDivider(Numerator, Denominator);
        Numerator /= delit;
        Denominator /= delit;
    }

    void Raise(int power) {
        Numerator = pow(Numerator, power);
        Denominator = pow(Denominator, power);
        Reduce();
    }

#pragma region Внутренние_операторы
   void operator = (myRatio a) {
       Numerator = a.GetNumerator();
       Denominator = a.GetDenominator();
    }
   void operator += (myRatio a) {
      myRatio bufRat = myRatio(Numerator,Denominator) + a;
      Numerator = bufRat.GetNumerator();
      Denominator = bufRat.GetDenominator();
   }
   void operator -= (myRatio a) {
       myRatio bufRat = myRatio(Numerator, Denominator) - a;
       Numerator = bufRat.GetNumerator();
       Denominator = bufRat.GetDenominator();
   }
   void operator *= (myRatio a) {
       myRatio bufRat = myRatio(Numerator, Denominator) * a;
       Numerator = bufRat.GetNumerator();
       Denominator = bufRat.GetDenominator();
   }
   void operator /= (myRatio a) {
       myRatio bufRat = myRatio(Numerator, Denominator) / a;
       Numerator = bufRat.GetNumerator();
       Denominator = bufRat.GetDenominator();
   }
   void operator ++() {
       Numerator += Denominator;
   }
   void operator --() {
       Numerator -= Denominator;
   }
#pragma endregion

};

#pragma region Опреторы_сравнения

bool operator == (myRatio a, myRatio b) {
    return a.GetValue() == b.GetValue();
}
bool operator != (myRatio a, myRatio b) {
    return a.GetValue() != b.GetValue();
}
bool operator > (myRatio a, myRatio b) {
    return a.GetValue() > b.GetValue();
}
bool operator < (myRatio a, myRatio b) {
    return a.GetValue() < b.GetValue();
}
bool operator >= (myRatio a, myRatio b) {
    return a.GetValue() >= b.GetValue();
}
bool operator <= (myRatio a, myRatio b) {
    return a.GetValue() <= b.GetValue();
}
#pragma endregion

#pragma region Операторы_преобразование

myRatio operator + (myRatio a, myRatio b) {
    int num, denom, numA = a.GetNumerator(), denomA = a.GetDenominator(), numB = b.GetNumerator(), denomB = b.GetDenominator();
    num = numA * denomB + numB * denomA;
    denom = denomA * denomB;
    myRatio rt(num, denom);
    rt.Reduce();
    return rt;
}
myRatio operator - (myRatio a, myRatio b)
{
    int num, denom, numA = a.GetNumerator(), denomA = a.GetDenominator(), numB = b.GetNumerator(), denomB = b.GetDenominator();
    num = numA * denomB - numB * denomA;
    denom = denomA * denomB;
    myRatio rt(num, denom);
    rt.Reduce();
    return rt;
}
myRatio operator * (myRatio a, myRatio b)
{
    int num, denom, numA = a.GetNumerator(), denomA = a.GetDenominator(), numB = b.GetNumerator(), denomB = b.GetDenominator();
    num = numA * numB;
    denom = denomA * denomB;
    myRatio rt(num, denom);
    rt.Reduce();
    return rt;
}
myRatio operator / (myRatio a, myRatio b)
{
    int num, denom, numA = a.GetNumerator(), denomA = a.GetDenominator(), numB = b.GetNumerator(), denomB = b.GetDenominator();
    num = numA * denomB;
    denom = denomA * numB;
    myRatio rt(num, denom);
    rt.Reduce();
    return rt;
}
#pragma endregion
