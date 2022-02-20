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

#pragma region ����������_���������
	void operator = (myRatio a);
	void operator += (myRatio a);
	void operator -= (myRatio a);
	void operator *= (myRatio a);
	void operator /= (myRatio a);
	void operator ++();
	void operator --();
#pragma endregion

};

#pragma region ���������_���������

bool operator == (myRatio a, myRatio b);
bool operator != (myRatio a, myRatio b);
bool operator > (myRatio a, myRatio b);
bool operator < (myRatio a, myRatio b);
bool operator >= (myRatio a, myRatio b);
bool operator <= (myRatio a, myRatio b);

#pragma endregion

#pragma region ���������_��������������

myRatio operator + (myRatio a, myRatio b);
myRatio operator - (myRatio a, myRatio b);
myRatio operator * (myRatio a, myRatio b);
myRatio operator / (myRatio a, myRatio b);

#pragma endregion
