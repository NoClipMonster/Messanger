#include <iostream>

//������ �����


class Welcome
{
private:
	char* m_data;
public:
	Welcome()
	{
		m_data = new char[14];
		const char* init = "Hello, World!";
		for (int i = 0; i < 14; ++i)
			m_data[i] = init[i];
	}
	~Welcome()
	{

//����� ������ ��� ����� \/ \/

	   	      delete[] m_data;
		// ���������� �����������
	}
	void print() const
	{
		std::cout << m_data;
	}
};
int main()
{
	Welcome hello;
	hello.print();
	hello.~Welcome();
	
	return 0;
}


//������ �����
class Fruit
{
public:
	std::string name;
	std::string color;
};
class Apple :public Fruit {

};
class Banana :Fruit
{

};
class GrannySmith :Apple
{
	GrannySmith() {
		name = "";
		color = "";
	}
};

//������ �����
class Shape {
	virtual void print() {
		std::ostream;
	}
	Shape operator<<(Shape sh) {

	}
public:
	virtual ~Shape() {

	}
};
class Triangle :Shape
{
	float dot1;
	float dot2;
	float dot3;
	~Triangle() {

	}
};
class Circle :Shape
{
	float dot;
	int radius;
	~Circle() {

	}
};

//�������� �����

template<class T>
class StringValuePair
{
	std::string m_length;
	T* m_data;
};
class Pair:StringValuePair
{

};
