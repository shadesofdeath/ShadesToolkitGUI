   M S B A M L   `   `   `  ���� H  DShadesToolkit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:ShadesToolkit.Views.PagesShadesToolkit.Views.Pages  &     ShadesToolkit.Views.Pages.Source   W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     9local'clr-namespace:ShadesToolkit.Views.Pages     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     A =Wpf.Ui, Version=3.0.4.0, Culture=neutral, PublicKeyToken=null/ui&http://schemas.lepo.co/wpfui/2022/xaml  -     N�Name$  
SourcePage�� N� Title$ ShadesToolkit��$��0,20,0,0q�   ApplicationBackgroundBrush  Wpf.Ui.Markup.Design   
Background# �     TextFillColorPrimaryBrush   
Foreground# �  .��� $	��Top=� � RowDefinitions �� $��80 ��� $��* �.+�� $��0��.+�  Wpf.Ui.Controls.Button  -     Name$ add_btn��$��0q�$��60��   CornerRadius$ 8�� � ContextMenu �� -    ��Name$ contextMenu��	 �� 	Placement$	 
MousePoint=�
 �� 	StaysOpen$
 False��.@�m� -   $��
Select WIM�� m� Icon    Wpf.Ui.Controls.SymbolIcon     Symbol$ FolderZip24=�m� -   $��
Select ISO��   $
 Cd16=�.���� .+�    Wpf.Ui.Controls.FontIcon   FontSize$ 20(�$ Add20=�$��0,8,0,0q�  Wpf.Ui.Controls.TextBlock  $	��Add��$��0,8,0,0q�  -   $ ISO_btn��$��0q�$��60��$ 8��$��10,0,0,0q�.���� .+�  $ 20(�$
 Cd16=�$��0,8,0,0q�  $��
Create ISO��$��0,8,0,0q�  -   $ convert_btn��$��0q�$��60��$ 8��$��10,0,0,0q�.���� .+�  $ 20(�$ ArrowSync20=�$��0,8,0,0q�  $��Convert��$��0,8,0,0q�  -   $ host_btn��$��0q�$��60��$ 8��$��10,0,0,0q�.���� .+�  $ 20(�$ Book24=�$��0,8,0,0q�  $��Host Editor��$��0,8,0,0q�  -	   $ iso_download_btn��$��0q�$��60��$ 8��$��10,0,0,0q�.���� .+�  $ 20(�$ DrawerArrowDownload20=�$��0,8,0,0q�  $��ISO Download��$��0,8,0,0q��� -
    ��Name$ CleanupMountsPanel��$y�Hidden=�$��1��$��0,80,0,0q�$��Center=�$��Center=�.+�"  Wpf.Ui.Controls.ProgressRing  -     Name$ DataProgress��   IsIndeterminate$
 True��$��42��$��42��  -     Name$ DataText��$)��#Cleaning in progress, please wait..��$��0,12,0,0q�
������� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     Color$ Black��   	Direction$	 320Q�   ShadowDepth$ 2Q�   Opacity$	 0.9Q�  Wpf.Ui.Controls.ListView  -     Name$ sourceListView��$��1��
������ $��0q� �� -   $ contextMenuControl��.@�	  Wpf.Ui.Controls.MenuItem	  -   $��Mount��   $ ArrowCurveUpRight20=�	  -   $��Unmount��   $ ArrowCurveDownLeft20=�	  -   $��Forget��   $ Delete20=�	  -   $��Open file directory��   $ FolderOpen24=�	  -   $��Open mount directory��   $ ArrowJoin20=�	  -   $��
Create ISO��   $
 Cd16=�	  -   $��
Export WIM��   $ FolderZip24=�	  $
��Edit�� 	  Icon   $ Edit24=�.@�	  -   $
��Name��   $ Clover24=�	  -   $��Description��   $ Clover24=�	  -   $��Flags��   $ Clover24=� �� View 
  Wpf.Ui.Controls.GridView
  .P�$  Wpf.Ui.Controls.GridViewColumn  $��Operating System�� �� Width$	 240�� �� DisplayMemberBinding �� *OS+  $��Architecture��$	 100�� �� *Architecture+  $��Flags��$	 160�� �� *Flags+  $��Index��$ 60�� �� *Index+  $
��Size��$	 180�� �� *Size+  $��Status��$	 135�� �� *Status+  $��Language��$
 Auto�� �� *
Language+  $��Last Changes��$
 Auto�� �� *LastChanges+