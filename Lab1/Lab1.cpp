// Lab1.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include "myRatio.h"
int main()
{
    myRatio r1;
    r1.ConsoleInput();

    std::cout << "Enter operator";
    char oper;
    std::cin >> oper;

    myRatio r2;
    r2.ConsoleInput();

    switch (oper)
    {
    case '+':
        (r1 + r2).ConsoleOutput();
        break;
    case '-':
        (r1 - r2).ConsoleOutput();
        break;
    case '*':
        (r1 * r2).ConsoleOutput();
        break;
    case '/':
        (r1 / r2).ConsoleOutput();
        break;

    default:
        std::cout << "\nUnknown operator";
        break;
    }
    
}

