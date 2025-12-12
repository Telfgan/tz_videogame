<?php
    require 'rb-mysql.php';
    R::setup( 'mysql:host=localhost;dbname=userdb', 'debian-sys-maint', 'Ilu4wNObdRHRlLJt' );

    if(R::testConnection() == false){
        echo 'Connect error';
        exit;
    }
?>