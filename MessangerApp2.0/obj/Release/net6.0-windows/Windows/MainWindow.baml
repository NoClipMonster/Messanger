   M S B A M L   `   `   `  ���� J  FMessangerApp2.0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:MessangerApp2._0MessangerApp2._0  !    MessangerApp2._0.MainWindow   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     0localclr-namespace:MessangerApp2._0       � Title$  Textator��$	��700��$	��900�� � WindowStartupLocation$ CenterScreen=�$	��900��$	��703��.��� -    �Name$ HeadGrid��.+�� -   $ MainGrid�����	#FF202020 � RowDefinitions �� �� $��50 � � ColumnDefinitions �� -    ��Name$ FirstColumnDef��$	��330 ��� .+�� -   $ TextEnterField�����	#FF28282D$��1��$��50��$��Center=�$��1��.+��� -    ��Name$ textInputBox�� �� TextWrapping$
 Wrap=�$��TextBox��$��	50,0,50,0q�
�������$��16(� �� AcceptsReturn$
 True��	 �� 
AcceptsTab$
	 True��.������ 
 �� Color$
 	#FFABADB3�� �� Opacity$ 0Q�.������ $
 White��$ 0Q�.���� $��Button��$
��Left=�$��50��$��50��$��Center=��� -   $��Button��$��Right=�$��50��$��50��$��Center=��� -    ��Name$ MessageScroller��$��1��$��Hidden=��� -    ��Name$ UsersButton��$��30��$��30��$	��Top=�$
��Left=�$��0,35,0,0q�$	��ᐊ���� -	   $ 
UsersField��$��Hidden=�$��2�����	#FF2D2D2D$��0��$��30,0,0,0q�$
��Left=�$	��300�� � ContextMenu �� .@�m� -
   $/��)Добавить пользователя��.��� -   $ ContactsGrid���� -   $ 
MenuButton��$��30��$��30��$	��Top=�$
��Left=�$��M��� -   $ MenuGrid��$
��Left=�$	��330��$��
-330,0,0,0q�$��2�����	#FF28282D.+��� -   $ 
ExitButton��$��
Выход��$��
65,0,65,10q�
����������	#FF5D5D5D$��36(�$��66��$��Bottom=���Z� �� -   $ CheFMesButton��$6��0Проверить новые сообщения��$��
10,10,10,0q�
����������	#FF5D5D5D$��66��$	��Top=�$��20(���Z� �� -   $ ClearAllMsgButton��$%��Очистить историю��$��
10,81,10,0q�
����������	#FF5D5D5D$��66��$	��Top=�$��20(���Z� 