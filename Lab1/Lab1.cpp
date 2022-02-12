// Lab1.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "Header.h"
int main()
{
    std::cout << "Hello World!\n";
    Ratio r(10,-2);
    Ratio r1;
    r1.ConsoleInput();
    r1.ConsoleOutput();
    Ratio r2;
    r2.SetNumerator(2);
    r2.SetDenominator(2);
    int r2N = r2.GetNumerator();
    int r2D = r2.GetDenominator();
    r1.Reduce();
    r1.ConsoleOutput();
    
    bool tr1 = (r + r1).GetValue() == r.GetValue() + r1.GetValue();
    bool tr2 = (r - r1).GetValue() == r.GetValue() - r1.GetValue();
    bool tr3 = (r * r1).GetValue() == r.GetValue() * r1.GetValue();
    bool tr4 = (r / r1).GetValue() == r.GetValue() / r1.GetValue();

    bool tr5 = r == r1;
    bool tr6 = r != r1;
    bool tr7 = r > r1;
    bool tr8 = r < r1;

}

