<?php
    require 'database.php';
    
    $login = $_POST['login'];
    $password = $_POST['password'];

    if(isset($login) == false || isset($password) == false){
        echo 'data struct error';
        exit;
    }
    $user = R::findOne('users', 'login = ?', array($login));

    if(isset($user) == false){
        echo 'Login error';
        exit;
    }

    if($user['password'] != $password){
        echo 'Password error';
        exit;
    }
    echo $user['id'];
?>