#pragma once
class myRatio
{
public:
	myRatio();
	myRatio(int num,int Denom);

	int GetNumerator();
	void SetNumerator(int num);

	int GetDenominator();
	void SetDenominator(int denom);

	void ConsoleInput();
	void ConsoleOutput();

	float GetValue();

	void Reduce();

	void Raise(int power);

#pragma region Внутренние_операторы
	void operator = (myRatio a);
	void operator += (myRatio a);
	void operator -= (myRatio a);
	void operator *= (myRatio a);
	void operator /= (myRatio a);
	void operator ++();
	void operator --();
#pragma endregion

};

#pragma region Операторы_сравнения

bool operator == (myRatio a, myRatio b);
bool operator != (myRatio a, myRatio b);
bool operator > (myRatio a, myRatio b);
bool operator < (myRatio a, myRatio b);
bool operator >= (myRatio a, myRatio b);
bool operator <= (myRatio a, myRatio b);

#pragma endregion

#pragma region Операторы_преобразование

myRatio operator + (myRatio a, myRatio b);
myRatio operator - (myRatio a, myRatio b);
myRatio operator * (myRatio a, myRatio b);
myRatio operator / (myRatio a, myRatio b);

#pragma endregion
