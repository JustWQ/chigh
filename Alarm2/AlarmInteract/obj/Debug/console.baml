   M S B A M L   `   `   `  ����H  DAlarmInteract, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:SCSV.AlarmInteractSCSV.AlarmInteract  5   	   $    SCSV.AlarmInteract.consoleDesk   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:SCSV.AlarmInteract     5   	     � Title$  console��5   	   $	��320��6   $	��260��6&    � Topmost . 
���  ��$��2q�6T    � ShowInTaskbar .   � AllowsTransparency .  J� Opacity$	 0.8Q�6�    � WindowStyle$
 None=�6�    � 
ResizeMode$ NoResize=�6�    � Triggers 5	      :� 5
   
    :� RoutedEvent$ Mouse.MouseLeave��6   .[�5      �� .��5      �� .��5      W� -   5        	 W�Name$	 DmouseLeave��5   &   
 g� Duration$
 0:0:1D�5   +    g� 	BeginTime$ 0:0:2X�5   +    �� TargetProperty$
 Left�5   +   5      5      5      :� 5   
   $ Mouse.MouseEnter��6   .[�5      �� .��5      �� .��5      W� -   5        $	 DmouseEnter��5   &   $
 0:0:0.2D�5   +   $
 Left�5   +   5      5      5      .��5       �� .+�5!   
   �� -   5         ��Name$ list_wating��5!      $��Hidden=�6,   5,      5.      5/      