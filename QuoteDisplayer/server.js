'use strict';
var path = require('path');
var express = require('express');
var fs = require("fs");
var md5 = require("md5");

var app = express();

function getRandomInt(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min) + min);
}

//who needs error handling?
var quotes = fs.readFileSync("./resources/quotes.txt", "utf8").toString().split("\n");

var staticPath = path.join(__dirname, '/');
app.use(express.static(staticPath));

// Allows you to set port in the project properties.
app.set('port', process.env.PORT || 8080);

var oldHash = "";
app.get("/quote", (req, res) => {
    //to make sure not to return the same quote
    //lets hope there is more than one quote :)
    var rand = getRandomInt(0, quotes.length);
    var newHash = md5(quotes[rand]);
    while (oldHash == newHash || quotes[rand].length <= 1) {
        rand = getRandomInt(0, quotes.length);
        newHash = md5(quotes[rand]);
    }
    oldHash = newHash;
    res.send(quotes[rand].trim());
    
});

var server = app.listen(app.get('port'), function () {
    console.log('listening');
});

