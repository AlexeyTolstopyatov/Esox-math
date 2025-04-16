<?php
/* Binary segments: 
 * endpoints declaration:
 *   server/linearAlgebra2
 * resources imports
 * */
?>

<?php
    session_start();
    include 'my_library2.php';
    if ($_SESSION['uwncb']==false)
    {
        echo "<H3>Решение неверное!</H3>";
        echo '<br/><font size="4"><b>Повтори теорию или сгенерируй другую систему</b></font>';
        echo '<br/><br/>';
        echo '<form  action="index.html">
        <td><input type = "submit" value="Сгенерировать другую систему"></td>';
    }
    else
    {
        $chr=$_GET["chr"];
        /*echo "выбрана цифра ".$chr;*/
        $A = $_SESSION['A'];
        $r = $_SESSION['r'];
        $n = $_SESSION['n'];
        $flag = 0;
        $_SESSION['flag']=$flag;
        /*echo " r = ".$r;*/
        if ($r!=$chr)
        {
            echo "<H3>Количество главных переменных неверно, попробуйте ещё раз</H3>";
            conclusion($A, $r, $n);
            echo '<br/><font size="4"><b>Выберите количество главных переменных: </b></font>';
            askr($n);
        }
        else
        {
            echo "<H3>Найдите общее решение неоднородной системы уравнений, представленное в виде НФСР</H3>";
            conclusion($A, $r, $n);
            asksys($r, $n);
        }
    }
?>
<?php
// Binary segment:
// imports declaration
// endpoints declaration
?>
<?php
    session_start();
    include 'my_library2.php';
    
    function checkXch($A, $Xch, $r, $n)
    {
        for ($i=0; $i<$n; $i++)
        {
            $q=0;
            for ($j=0; $j<$n; $j++)
            {
                $q+=$Xch[$j]*$A[$i][$j];
            }
            if ($q!=$A[$i][$n]) return false;
        }
        return true;
    }
    
    function checkX($A, $X, $r, $n)
    {
        for ($k=0; $k<$n-$r; $k++)
        {
            for ($i=0; $i<$n; $i++)
            {
                $q=0;
                for ($j=0; $j<$n; $j++)
                {
                    $q+=$X[$k][$j]*$A[$i][$j];
                }
                if ($q!=0) return false;
            }
        }
        return true;
    }
    
    function checkDet($A, $X, $R, $r, $n)
    {
        $Xr = array($n-$r);
        for ($i=0; $i<$n-$r; $i++)
        {
            $Xr[$i]=array($n-$r);
        }
        $q=0;
        for ($i=0; $i<$n-$r; $i++)
        {
            for ($j=0; $j<$n; $j++)
            {
                if (findN($j+1, $R)==false)
                {
                    $Xr[$i][$q]=$X[$i][$j];
                    $q++;
                }
            }
            $q=0;
        }
        /*echo "Xr<br/>";
        for ($i=0; $i<$n-$r; $i++)
        {
            for ($j=0; $j<$n-$r; $j++)
            {
                echo $Xr[$i][$j]."&nbsp";
            }
            echo '<br/>';
        }*/
        if ($n-$r==1)
        {
            if ($Xr[0][0]!=0) return true;
        }
        elseif ($n-$r==2)
        {
            $q=$Xr[0][0]*$Xr[1][1]-$Xr[1][0]*$Xr[0][1];
            if ($q!=0) return true;
        }
        elseif ($n-$r==3)
        {
            $q=$Xr[0][0]*$Xr[1][1]*$Xr[2][2]-$Xr[0][0]*$Xr[1][2]*$Xr[2][1]-$Xr[0][1]*$Xr[1][0]*$Xr[2][2];
            $q=$q+$Xr[0][1]*$Xr[1][2]*$Xr[2][0]+$Xr[0][2]*$Xr[1][0]*$Xr[2][1]-$Xr[0][2]*$Xr[1][1]*$Xr[2][0];
            if ($q!=0) return true;
        }
        return false;
    }
    
    $A = $_SESSION['A'];
    $r = $_SESSION['r'];
    $n = $_SESSION['n'];
    $flag = $_SESSION['flag'];
    $R=array($r);
    $ch=true;
    for ($i=0; $i<$r; $i++)
    {
        $R[$i]=$_GET["r".($i)];
        if (!is_numeric($R[$i])) $ch=false;
        if ($R[$i]==NULL) $ch=false;
    }
    /*echo "R<br/>";
    for ($i=0;$i<$r; $i++) echo $R[$i]."   ";
    echo '<br/>';*/
    
    $Xch=array($n);
    for ($i=0; $i<$n; $i++)
    {
        $Xch[$i]=$_GET["xc".($i)];
        if (!is_numeric($Xch[$i])) $ch=false;
        if ($Xch[$i]==NULL) $ch=false;
    }
    /*echo "Xch<br/>";
    for ($i=0;$i<$n; $i++) echo $Xch[$i]."   ";
    echo '<br/>';*/
    
    $X = array($n-$r);
    for ($i=0; $i<($n-$r); $i++)
    {
        $X[$i]=array($n);
    }
    for ($i=0; $i<($n-$r); $i++)
    {
        for ($j=0; $j<$n; $j++)
        {
            $X[$i][$j]=$_GET["x".($j+($i*$n))];
            if (!is_numeric($X[$i][$j])) $ch=false;
            if ($X[$i][$j]==NULL) $ch=false;
        }
    }
    /*echo "X<br/>";
    for ($i=0; $i<($n-$r); $i++)
    {
        for    ($j=0; $j<$n; $j++) echo $X[$i][$j]."   ";
        echo "<br/>";
    }
    echo '<br/>';*/
    /*if ($ch==false) echo " ch = false<br/>";
    elseif ($ch==true) echo " ch = true<br/>";*/
    if ($ch==false)
    {
        if ($flag<2)
        {
            $flag++;
            $_SESSION['flag']=$flag;
            echo "<H3>Решение неверно, попробуйте снова</H3>";
            conclusion($A, $r, $n);
            asksys($r, $n);
        }
        else
        {
            echo "<H3>Решение неверное!</H3>";
            echo '<br/><font size="4"><b>Повтори теорию или сгенерируй другую систему</b></font>';
            echo '<br/><br/>';
            echo '<form  action="index.html">
            <td><input type = "submit" value="Сгенерировать другую систему"></td>';
            $uwncb=false;
            $_SESSION['uwncb']=$uwncb;
        }
    }
    else
    {
        if (checkXch($A, $Xch, $r, $n)==false) $ch=false;
        /*if (checkXch($A, $Xch, $r, $n)==false) echo " Xch = false<br/>";
        elseif (checkXch($A, $Xch, $r, $n)==true) echo " Xch = true<br/>";*/
        if (checkX($A, $X, $r, $n)==false) $ch=false;
        /*if (checkX($A, $X, $r, $n)==false) echo " X = false<br/>";
        elseif (checkX($A, $X, $r, $n)==true) echo " X = true<br/>";*/
        if (checkDet($A, $X, $R, $r, $n)==false) $ch=false;
        /*if (checkDet($A, $X, $R, $r, $n)==false) echo " Det = false<br/>";
        elseif (checkDet($A, $X, $R, $r, $n)==true) echo " Det = true<br/>";*/
        if ($ch==false)
        {
            if ($flag<2)
            {
                $flag++;
                $_SESSION['flag']=$flag;
                echo "<H3>Решение неверно, попробуйте снова</H3>";
                conclusion($A, $r, $n);
                asksys($r, $n);
            }
            else
            {
                echo "<H3>Решение неверное!</H3>";
                echo '<br/><font size="4"><b>Повтори теорию или сгенерируй другую систему</b></font>';
                echo '<br/><br/>';
                echo '<form  action="index.html">
                <td><input type = "submit" value="Сгенерировать другую систему"></td>';
                $uwncb=false;
                $_SESSION['uwncb']=$uwncb;///
            }
        }
        else
        {
            echo "<H3>Решение верное! Поздравляем!</H3>";
            echo '<form  action="index.html">
            <td><input type = "submit" value="Сгенерировать другую систему"></td>';
        }
    }
?>
<?php
// binary segment
?>

<?php
session_start();
function randoma($matr, $str, $coll)//задаем значения 
{
    
    $matr=array();
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $coll; $j++)
        {
            $matr[$i][$j] = rand(-8,8);
        }
    }
    $_SESSION['matr'] = $matr;
    ?><html><a href="result.php"></a></html>
    <?php 
    return $matr;
}
function randomx( $x,$str) //задаем значения неизвестных
{
    for ($i = 0; $i < $str; ++$i)
    {
            $x[$i]= rand(-5,5);
    }
    return $x;
}
function coutx($str, $x) //вывод значений х
{
    for ($k = 0; $k < $str; ++$k)
    {
        echo
        "<i>x</i>"."<sub>".($k+1)."</sub> = ".$x[$k]."<br>";
    }
}
function resyltat($str, $matr, $coll)//проверка( в с++)
{
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $coll; $j++)
        {
           echo $matr[$i][$j]. "  ";            
        }
        echo  "<br>";
    }    
}
function brez($a, $str, $coll)
{
    $b=array(0,0,0,0,0,0);
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $coll; $j++)
        {
            $b[$i] = $b[$i] + $a[$i][$j];
        }
    }
    $_SESSION['b'] = $b;
    return $b;
}
function proverka($n, $xn, $a, $m, $x)
    {
        
        for ($i = 0; $i < $n; $i++)
        {
            for ($j = 0; $j < $m; $j++)
            {
                 $a[$i][$j]=$x[$j]*$a[$i][$j];
            }
        }
        return $a;
    }
function systemaE($nach, $x, $b, $str, $coll)
{
   //echo "<p>". "<img src='int.png'>"."</p>";       
    for ($i = 0; $i < $str; ++$i)
        {
        $flag=false;
            for ($j = 0; $j < $coll; ++$j)
            {
                
                if ($nach[$i][$j] !=0)
                {    
                    if ($j == ($coll - 1))
                    {
                        
                        if ($nach[$i][$coll - 1] == 1)
                        {
                            echo " + "."<i>x</i><sub>".($j + 1)."</sub>" . " = " . $b[$i]. "<br>"; 
                        }
                        else if ($nach[$i][$coll - 1] > 0)
                        {
                            echo " + ";
                            echo $nach[$i][$j]."<i>x</i><sub>" . ($j + 1) . "</sub>" . " = " . $b[$i] . "<br>";
                        }
                        else if ($nach[$i][$coll - 1] == -1)
                        {
                            echo " - ";
                            echo "<i>x</i><sub>" . ($j + 1) . "</sub>" . " = " . $b[$i] . "<br>";
                        }
                        else if ($nach[$i][$coll - 1] < 0)
                        {
                            echo "-";
                            echo abs($nach[$i][$j]) . "<i>x</i><sub>" . ($j + 1) . "</sub>" . " = " . $b[$i] . "<br>";
                            
                        }
                    }
                    else  if ($nach[$i][$j] == 1)
                     {
                        if ($j == 0)
                        {
                            echo "<i>x</i><sub>" . ($j + 1) . "</sub>";
                        }
                        else
                        {
                            echo " + ";
                            echo "<i>x</i><sub>" . ($j + 1) . "</sub>";
                        }
                    }
                    else if ($nach[$i][$j] == -1)
                    {
                        echo" - ";
                        echo "<i>x</i><sub>" . ($j + 1) . "</sub>";
                    }
                    else if ($j == 0)
                    {
                        echo $nach[$i][$j] . "<i>x</i><sub>" . ($j + 1) . "</sub>";
                    }
                    else if ($nach[$i][$j] < 0)
                    {
                        echo " - ";
                        echo abs($nach[$i][$j]) . "<i>x</i><sub>" . ($j+1) . "</sub>";
                    }
                    else if ($nach[$i][$j] > 0)
                    {
                        if($flag)
                        {
                            echo  $nach[$i][$j] . "<i>x</i><sub>" . ($j+1) . "</sub>";
                        }
                        else
                        {
                            echo " + ";
                            echo  $nach[$i][$j] . "<i>x</i><sub>" . ($j+1) . "</sub>";
                        }
                    }
                }
                else if($j == 0)
                {
                    $flag=true;
                }
                else if ($nach[$i][$coll - 1] == 0)
                {
                    echo " = " . $b[$i] . "<br>";
                }
            }
            echo "<br>";
        }
}
function rezX($str)
{   echo "<H3>"."Введите полученные ответы и проверьте себя"."</H3>";
    for($i=0;$i<$str;$i++) {
        echo "<form  action=\"result.php\">";
        echo "<th> <i> x </i><sub>".($i+1)."</sub>"."=". "<input type=\"text\" size =\"5\" name=\"x".($i + 1)."\"> </th>";
 }
 ?>
    <br>
    <br>
    <input type = "submit" value="Отправить">
    </form>
    </html>
    <?php    
}
$l;
$n=$_GET["n"];
$_SESSION['n'] = $n;
$m=$_GET["m"];
$_SESSION['m'] = $m;
if($n>$m)
    {
        $str=$m;
        $coll=$n;
    }
    else 
    {
        $str=$n;
        $coll=$m;
    }
if(($coll>1)&&($str>1)&&($coll<6)&&($str<6))
    {
        echo "<H3>"."Решите систему уравнений:"."</H3>";
        $matr=array();
        $x=array();
        $b2=array();
        $nach=array();
        $nach=randoma($matr, $str, $coll);
        $x=randomx($x, $n);
        $matr = proverka($str, $str, $nach, $coll, $x);
        $b2=brez($matr, $str, $coll);
        systemaE($nach, $x, $b2, $str, $coll);
        rezX($n);
    }
    else
    {
        echo "<H3>"."Введите корректные значения системы"."</H3>";?>
        <html>
        <form  action="function.php">
        <table>
        <td>Введите колличество строк </td> <th> <i> n </i>= <input type="text" size ="5" name="n"> </th>
        <tr>
        <td>Введите колличество столбцов </td> <th> <i>m </i>= <input type="text" size ="5" name="m"> </th>
        </tr>
        </table>
        <br>
        <input type = "submit" value="Отправить">
        </form></html><?php   
    }
?>
<?php
/* binary segment:
 *
 * imports declaration
 */
?>

<?php
session_start();
function randomxE( $x,$str) //задаем значения неизвестных
{
    for ($i = 0; $i < $str; ++$i)
    {
        $x[$i]= rand(-5,5);
    }
    return $x;
}
function randomaE($a, $n)
{
    for ($i = 0; $i < $n; $i++)
    {
        for ($j = 0; $j < $n; $j++)
        {
            if ($i == $j)
            {
                $a[$i][$j] = 1;
            }
            else
            {
                $a[$i][$j] = 0;
            }
        }
    }
    return $a;
}
function coutxE($str, $x) //вывод значений х
{
    for ($k = 0; $k < $str; ++$k)
    {
        echo
        "<i>x</i>"."<sub>".($k+1)."</sub> = ".$x[$k]."<br>";
    }
}
function proverkaE($n, $a, $x)
    {
        
        for ($i = 0; $i < $n; $i++)
        {
            for ($j = 0; $j < $n; $j++)
            {
                $a[$i][$j]=$x[$j]*$a[$i][$j];
            }
        }
        return $a;
    }
function preobrozobanueE($a, $n)
{    //echo "n=".$n."<br>";
    $u = array();
    for ($k = 0; $k < $n; $k++)
    {
        
        $u[$k] = rand(-3,3) ;
        if ($u[$k]== 0) { $u[$k]++; }
        //echo "u[".$k."]=".$u[$k]."<br>"; 
        
    }
    $tmp=array();
        for ($g = 0; $g < $n; $g++)
        {
            for ($y = 0; $y < $n; $y++)
            {
            //echo "g=".$g."<br>";
            //echo "u[".$g."]=".$u[$g]."<br>";
            //echo "a[".$g."][".$y."]=".$a[$g][$y]."<br>";    
                $tmp[$g] = ($u[$g])*($a[$g][$y]);
              //  echo "tmp[".$g."]=".$tmp[$g];
                for ($t = 0; $t < $n; $t++)
                {
                    if ($g != $t)
                    {
                        $a[$t][$y] = $a[$t][$y] + $tmp[$g];
                    }
                }
                //echo "<br>";
            }
            //echo "<br>";
    }
    $_SESSION['matr'] = $a;
    return $a;
}    
function brezE($a, $str)
{
    $b=array(0,0,0,0,0,0);
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $str; $j++)
        {
            $b[$i] = $b[$i] + $a[$i][$j];
        }
    }
    return $b;
}
    function rezX($str)
    {  
        echo "<H3>"."Введите любое частное решение"."</H3>";
        for($i=0;$i<$str;$i++) 
        {
            echo "<form  action=\"result.php\">";
            echo "<th> <i> x </i><sub>".($i+1)."</sub>"."=". "<input type=\"text\" size =\"5\" name=\"x".($i + 1)."\"> </th>";
        } 
        ?>
    <br>
    <br>
    <input type = "submit" value="Отправить">
    </form>
    </html>
    <?php    
}
    $l;
    $str=$_GET["n"];
    $_SESSION['n'] = $str;
    $_SESSION['m'] = $str;
    include 'my_library.php';
    if(($str>1)&&($str<6))
    {
        echo "<H3>"."Решите систему уравнений:"."</H3>";
        $matr=array();
        $x=array();
        $b2=array();
        $nach=array();
        $nach=randomaE($matr, $str);
        $x=randomxE($x, $str);
        //coutxE($str, $x);
        $matr=proverkaE($str, $nach, $x);
        $matr=preobrozobanueE($nach, $str);
        $b2=brezE($matr,$str);
        if($str==2)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"75\">"."</a>"."</td>".
            "<td>";
        }
        else if($str==3)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"115\">"."</a>"."</td>".
            "<td>";
        }
        else if($str==4)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"150\">"."</a>"."</td>".
            "<td>";
        }
        else if($str==5)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"195\">"."</a>"."</td>".
            "<td>";
        }
        echo "<br>";
        systemaE($matr, $b2, $str, $str);
        echo "</td>"."</table>";
        rezX($str);
    }
    else
    {
        echo "<H3>"."Введите корректные значения системы"."</H3>";?>
        <html>
        <form  action="function_E.php">
        <table>
        <td>Введите колличество строк </td> <th> <i> n </i>= <input type="text" size ="5" name="n"> </th>
        <tr>
        <td>Введите колличество столбцов </td> <th> <i>m </i>= <input type="text" size ="5" name="m"> </th>
        </tr>
        </table>
        <br>
        <input type = "submit" value="Отправить">
        </form></html><?php      
    }
?>

<?php  
// hide declaration 
?>

<?php
//для function
session_start();
function randoma($matr, $str, $coll)//задаем значения 
{
    /* echo "str=".$str;
    echo "<br>";
    echo "coll=".$coll;
    echo "<br>";*/
    $matr=array();
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $coll; $j++)
        {
            $matr[$i][$j] = rand(-8,8);
           // echo $matr[$i][$j]."  ";
        }
        //echo "<br>";
    }
    $_SESSION['matr'] = $matr;
    ?>
    <html><a href="result.php"></a></html>
    <?php 
    return $matr;
}
function randomx( $x,$str) //задаем значения неизвестных
{
    for ($i = 0; $i < $str; ++$i)
    {
        $x[$i]= rand(-5,5); 
    }
    return $x;
}
function coutx($str, $x) //вывод значений х
{
    for ($k = 0; $k < $str; ++$k)
    {
        echo
        "<i>x</i>"."<sub>".($k+1)."</sub> = ".$x[$k]."<br>";
    }
}
function resyltat($str, $matr, $coll)//проверка( в с++)
{
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $coll; $j++)
        {
           echo $matr[$i][$j]. "  ";            
        }
        echo  "<br>";
    }    
}
function brez($a, $str, $coll)
{
    $b=array(0,0,0,0,0,0);
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $coll; $j++)
        {
            $b[$j] = $b[$j] + $a[$i][$j];
            //echo "b[".($j+1)."]=".$b[$j];  
        }
    }
    $_SESSION['b'] = $b;
    return $b;
}
function proverka($n, $xn, $a, $m, $x)
    {
        for ($i = 0; $i < $n; $i++)
        {
            for ($j = 0; $j < $m; $j++)
            {
                $a[$i][$j]=$x[$i]*$a[$i][$j];
            }
        }
        return $a;
    }
//для function_E
function randomxE( $x,$str) //задаем значения неизвестных
{
    for ($i = 0; $i < $str; ++$i)
    {
        $x[$i]= rand(-5,5);
    }
    return $x;
}/*
function randomaE($matr, $str)//задаем значения 
{
    
    $matr=array();
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $str; $j++)
        {
            $matr[$i][$j] = rand(-8,8);
        }
    }
    $_SESSION['matr'] = $matr;
    ?><html><a href="result.php"></a></html>
    <?php 
    return $matr;
}
*/
function randomaE($a, $n)
{
    for ($i = 0; $i < $n; $i++)
    {
        for ($j = 0; $j < $n; $j++)
        {
            if ($i == $j)
            {
                $a[$i][$j] = 1;
            }
            else
            {
                $a[$i][$j] = 0;
            }
        }
    }
    
    return $a;
}
function coutxE($str, $x) //вывод значений х
{
    for ($k = 0; $k < $str; ++$k)
    {
        echo
        "<i>x</i>"."<sub>".($k+1)."</sub> = ".$x[$k]."<br>";
    }
}
function proverkaE($n, $a, $x)
    {
        for ($i = 0; $i < $n; $i++)
        {
            for ($j = 0; $j < $n; $j++)
            {
                $a[$i][$j]=$x[$j]*$a[$i][$j];
            }
        }
        return $a;
    }
function preobrozobanueE($a, $n)
{
    $u = array();
    for ($k = 0; $k < $n; $k++)
    { 
        $u[$k] = rand(-3,3) ;
        if ($u[$k]== 0) 
        { 
            $u[$k]++;
        }
    }
    $tmp=array();
        for ($g = 0; $g < $n; $g++)
        {
            for ($y = 0; $y < $n; $y++)
            {
                $tmp[$g] = ($u[$g])*($a[$g][$y]);
                for ($t = 0; $t <$n; $t++)
                {
                    if ($g != $t)
                    {
                        $a[$t][$y] = $a[$t][$y] + $tmp[$g];        
                    }
                }
            }
        }
        
    $_SESSION['matr'] = $a;
    return $a;
}    
function brezE($a, $str)
{
    $b=array(0,0,0,0,0,0);
    for ($i = 0; $i < $str; $i++)
    {
        for ($j = 0; $j < $str; $j++)
        {
            $b[$i] = $b[$i] + $a[$i][$j];
        }
    }
    $_SESSION['b'] = $b;
    return $b;
}
 /*   if($_GET['vehicle']==="one")
    {
        FUN_E();
    }
    else if($_GET['vehicle']==="set")
    {
        FUN();
    }
    */
function rezX($str)
{   echo "<H3>"."Введите решение системы"."</H3>";
    for($i=0;$i<$str;$i++) {
        echo "<form  action=\"result_x1.php\">";
        echo "<th> <i> x </i><sub>".($i+1)."</sub>"."=". "<input type=\"text\" size =\"5\" name=\"x".($i + 1)."\"> </th>";
        $flag=1;
        $_SESSION['flag'] = $flag;
 }
 ?>
        <br>
        <br>
        <table>
        <tr>
        <td><input type = "submit" value="Отправить"></td>
        </form>
        <form  action="index.html">
        <td><input type = "submit" value="Сгенерировать другую систему"></td>
        </tr>
        </form>
        </table>
    <?php    
}
/*function FUN()
{    
    $l;
    $n=$_GET["n"];
    $_SESSION['n'] = $n;
    $m=$_GET["m"];
    $_SESSION['m'] = $m;
    if($n>$m)
    {
        $str=$m;
        $coll=$n;
    }
    else 
    {
        $str=$n;
        $coll=$m;
    }
    if(($coll>1)&&($str>1)&&($coll<6)&&($str<6))
    {
        echo "<H3>"."Решите систему уравнений:"."</H3>";
        $matr=array();
        $x=array();
        $b2=array();
        $nach=array();
        $nach=randoma($matr, $str, $coll);
        $x=randomx($x, $n);
        $matr = proverka($str, $str, $nach, $coll, $x);
        $b2=brez($matr, $str, $coll);
        systemaE($nach, $x, $b2, $str, $coll);
        rezX($n);
    }
    else
    {
        echo "<H3>"."Введите корректные значения размеров системы"."</H3>";?>
        <html>
        <form  action="function.php">
        <table>
        <td>Введите количество неизвестных (<i>n</i> ϵ [2,5]) </td> <th> <i> n </i>= <input type="text" size ="5" name="n"> </th>
        <tr>
        <td>Введите количество уравнений (<i>m</i> ϵ [2,5])</td> <th> <i>m </i>= <input type="text" size ="5" name="m"> </th>
        </tr>
        </table>
        <br>
        <table>
        <tr>
        <td><input type = "submit" value="Отправить"></td>
        </form>
        <form  action="index.html">
        <td><input type = "submit" value="Сгенерировать другую систему"></td>
        </tr>
        </form>
        </table>
        <?php
    }
}*/
    $l;
    $str=$_GET["n"];
    $_SESSION['n'] = $str;
    $_SESSION['m'] = $str;
    include 'my_library.php';
    if(($str>1)&&($str<6))
    {
        echo "<H3>"."Решите систему уравнений:"."</H3>";
        $matr=array();
        $x=array();
        $b2=array();
        $nach=array();
        $nach=randomaE($matr, $str);
        $x=randomx($x, $str);
        //coutxE($str, $x);
        $matr=preobrozobanueE($nach, $str);
        $matr1=proverkaE($str, $matr, $x);
        $b2=brezE($matr1,$str);
        if($str==2)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"75\">"."</a>"."</td>".
            "<td>";
        }
        else if($str==3)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"115\">"."</a>"."</td>".
            "<td>";
        }
        else if($str==4)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"150\">"."</a>"."</td>".
            "<td>";
        }
        else if($str==5)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"195\">"."</a>"."</td>".
            "<td>";
        }
        echo "<br>";
        systemaE($matr, $b2, $str, $str);
        echo "</td>"."</table>";
        rezX($str);
    }
    else
    {
        echo "<H3>"."Введите корректные значения размеров системы"."</H3>";?>
        <html>
        <form  action="general_function.php">
        <table>
        <td>Введите количество неизвестных (<i>n</i> ϵ [2,5])</td> <th> <i> n </i>= <input type="text" size ="5" name="n"> </th>
        </table>
        <br>
        <table>
        <tr>
        <td><input type = "submit" value="Отправить"></td>
        </form>
        <form  action="index.html">
        <td><input type = "submit" value="Сгенерировать другую систему"></td>
        </tr>
        </form>
        </table><?php        
    }
?>
<?php
// index.html declaration set
?>
<html>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<head>
<body>
<title>
Линейная алгебра для радиофизиков!
</title>
<center>
<H1>
<b>
Вас приветствует генератор систем линейных уравнений!
<br>
</b>
</H1>
</center>
</body>
<form  action="parametres.php">
<H3> Какую систему необходимо сгенерировать? </H3> 
<input id = "radio_one" type="radio" name="vehicle" value="one" checked> 
<label for = "radio_one" > Квадратную систему уравнений с единственным решением </label> <br> <br> 
<input id = "radio_set" type="radio" name="vehicle" value="set">
<label for = "radio_set" >Систему, имеющую несколько решений<label> <br> <br> 
<input type = "submit" value="Отправить">
</form>
</head>
</html>

<?php
function systemaE($nach, $b, $str, $coll)
{
   //echo "<p>". "<img src='int.png'>"."</p>";
   //print_r($nach);
   //echo "<br>";
   //print_r($b);
   //echo "<br>";
   //print_r($str);
   //echo "<br>";
   //print_r($coll);
   //echo "<br>";
    for ($i = 0; $i < $str; ++$i)
        {
        $flag = false; // false -- ведущих элементов не было, true -- ведущие элементы были
            for ($j = 0; $j < $coll; ++$j)
            {   
                if ($nach[$i][$j] != 0)
                {
                    if ($j == ($coll - 1))
                    {
                        
                        if ($nach[$i][$coll - 1] == 1)
                        {
                            echo " + "."<i>x</i><sub>".($j + 1)."</sub>" . " = " . $b[$i]. "<br>"; 
                        }
                        else if ($nach[$i][$coll - 1] > 0)
                        {
                            echo " + ";
                            echo $nach[$i][$j]."<i>x</i><sub>" . ($j + 1) . "</sub>" . " = " . $b[$i] . "<br>";
                        }
                        else if ($nach[$i][$coll - 1] == -1)
                        {
                            echo " - ";
                            echo "<i>x</i><sub>" . ($j + 1) . "</sub>" . " = " . $b[$i] . "<br>";
                        }
                        else if ($nach[$i][$coll - 1] < 0)
                        {
                            echo "-";
                            echo abs($nach[$i][$j]) . "<i>x</i><sub>" . ($j + 1) . "</sub>" . " = " . $b[$i] . "<br>";
                            
                        }
                    }
                    else  if ($nach[$i][$j] == 1)
                     {
                        if ($flag)
                        {
                            echo " + ";
                            echo "<i>x</i><sub>" . ($j + 1) . "</sub>";
                        }
                        else
                        {
                            echo "<i>x</i><sub>" . ($j + 1) . "</sub>";
                        }
                    }
                    else if ($nach[$i][$j] == -1)
                    {
                        echo" - ";
                        echo "<i>x</i><sub>" . ($j + 1) . "</sub>";
                    }
                    else if ($j == 0)
                    {
                        echo $nach[$i][$j] . "<i>x</i><sub>" . ($j + 1) . "</sub>";
                    }
                    else if ($nach[$i][$j] < 0)
                    {
                        echo " - ";
                        echo abs($nach[$i][$j]) . "<i>x</i><sub>" . ($j+1) . "</sub>";
                        $flag=false;
                    }
                    else if ($nach[$i][$j] > 0)
                    {
                        if($flag)
                        {
                            echo " + ";
                            echo  $nach[$i][$j] . "<i>x</i><sub>" . ($j+1) . "</sub>";
                            //$flag=false;
                        }
                        else
                        {
                            echo  $nach[$i][$j] . "<i>x</i><sub>" . ($j+1) . "</sub>";
                        }
                    }
                    $flag = true;
                }
                else if ($j == $coll - 1 && $nach[$i][$j] == 0 && $flag)
                {
                    echo " = " . $b[$i] . "<br>";
                }
            }
            echo "<br>";
        }
}
?>
<?php
   function conclusion($A, $r, $n)
    {
        if($n==4)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"100\">"."</a>"."</td>".
            "<td>";
        }
        else if($n==5)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"115\">"."</a>"."</td>"."<td>";
        }
        else if($n==6)
        {
            echo
            "<table>".
            "<td>"."<img src=\"1_sys.png\"". 
            "width=\"20\" height=\"145\">"."</a>"."</td>"."<td>";
        }
        for ($i=0; $i<$n; $i++)
        {
            $flag=true;
            for ($j=0; $j<$n; $j++)
            {
                if ($flag==false)
                {
                    if($A[$i][$j]!=0)
                    {
                        if ($A[$i][$j]==1) echo "&nbsp+&nbsp"."<i>x</i><sub>".($j + 1)."</sub>";
                        if ($A[$i][$j]==-1) echo "&nbsp-&nbsp"."<i>x</i><sub>".($j + 1)."</sub>";
                        if($A[$i][$j]<0 && $A[$i][$j]!=-1) echo "&nbsp-&nbsp".abs($A[$i][$j])."<i>x</i><sub>".($j + 1)."</sub>";
                        if($A[$i][$j]>0 && $A[$i][$j]!=1) echo "&nbsp+&nbsp".$A[$i][$j]."<i>x</i><sub>".($j + 1)."</sub>";
                    }
                }
                if ($flag==true)
                {
                    if($A[$i][$j]!=0)
                    {
                        if ($A[$i][$j]==1) echo "&nbsp&nbsp&nbsp"."<i>x</i><sub>".($j + 1)."</sub>";
                        if ($A[$i][$j]==-1) echo "&nbsp-&nbsp"."<i>x</i><sub>".($j + 1)."</sub>";
                        if($A[$i][$j]<0 && $A[$i][$j]!=-1) echo "&nbsp-&nbsp".abs($A[$i][$j])."<i>x</i><sub>".($j + 1)."</sub>";
                        if($A[$i][$j]>0 && $A[$i][$j]!=1) echo "&nbsp&nbsp&nbsp".$A[$i][$j]."<i>x</i><sub>".($j + 1)."</sub>";
                        $flag=false;
                    }
                }
            }
            if ($A[$i][$n]<0) echo "&nbsp=&nbsp"."&nbsp-&nbsp".abs($A[$i][$n]).'<br/>';
            elseif ($A[$i][$n]>=0) echo "&nbsp=&nbsp"."&nbsp&nbsp&nbsp".$A[$i][$n].'<br/>';
        }
        echo "</table>";
    }
    function askr($n)
    {
        echo '<table><tr>';
        echo "<form action=\"ask.php\">";
        echo '<select name="chr">';
        for ($i=1; $i<=$n; $i++) 
        {
            echo    "<option value=$i>$i";
        }
        echo '</option></select>';
        echo '<br/><br/>';
        echo '<td><input type = "submit" value="Отправить"></td>
        </form>';
        echo '<form  action="index.html">
        <td><input type = "submit" value="Сгенерировать другую систему"></td>';
        echo '</tr></table></form>';
    }
    function asksys($r, $n)
    {
        echo "<form  action=\"check.php\">";
        echo '<br/><font size="4"><b>Введите номера главных переменных: </b></font>';
        for ($i=0; $i<$r; $i++)
        {
            echo "<th><input type=\"text\" size =\"5\" name=\"r".($i)."\"> </th>";
        }
        echo '<br/><br/>';
        
        echo '<br/><font size="4"><b>Введите частное решение данной неоднородной системы: </b></font>';
        echo '<br/><br/>';
        for($i=0;$i<$n;$i++) 
        {
            echo "<form  action=\"result.php\">";
            echo "<th> <i> x </i><sub>".($i+1)."</sub>"."="."<input type=\"text\" size =\"5\" name=\"xc".($i)."\"> </th>";
        }
        echo '<br/><br/>';
        $TEXT = array(
            0 => "первое",
            1 => "второе",
            2 => "третье",
            );
        for ($i=0;$i<($n-$r); $i++)
        {
            echo '<br/><font size="4"><b>Введите '.$TEXT[$i].' решение НФСР: </b></font>';
            echo '<br/><br/>';
            for($j=0;$j<$n;$j++) 
            {
                echo "<form  action=\"result.php\">";
                echo "<th> <i> x </i><sub>".($j + 1)."</sub>"."="."<input type=\"text\" size =\"5\" name=\"x".($j +($i*$n))."\"> </th>";
            }
            echo '<br/><br/>';
        }
        echo '<table><tr>';
        echo '<td><input type = "submit" value="Отправить"></td>
        </form>';
        echo '<form  action="index.html">
        <td><input type = "submit" value="Сгенерировать другую систему"></td>';
        echo '</tr></table>';
    }
    
    function findN($n, $N)#проверяет наличие числа в массиве, возвращает true/false
    {
        foreach($N as $value)
        {
            if ($n==$value) return true;
        }
        return false;
    }
    
?>
<!-- imports declaration
  -- 