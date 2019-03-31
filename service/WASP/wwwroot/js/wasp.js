$(document).ready(function () {
    init();
});

function init() {
    $("#search").submit(function (event) {
        event.preventDefault();
        var needle = $("#searchbox")[0].value;
        var jqxhr = $.get("api/SearchAttacks?needle=" + needle,
            function (data) {
                var container = $("#results-container");
                while (container[0].firstChild) {
                    container[0].removeChild(container[0].firstChild);
                }
                var table = $('<table id="results-table"></table>');
                var matches = data["matches"];
                header = `<tr><th>Description</th><th>Action</th></tr>`;
                table.append($(header));
                for (var match in matches) {
                    var attack = matches[match];
                    console.log(attack);
                    var description = attack.content.content;
                    row = `<tr><td>${description}</td><td><button onclick="inspectAttack(${attack.id})">Inspect</button></tr>`;
                    table.append($(row));
                }
                container.append($(table));
            });
    });

    $("#add").submit(function (event) {
        event.preventDefault();
        console.log("Adding new attack");
        var date = $("#input-date")[0].value;
        var location = $("#input-location")[0].value;
        var description = $("#input-description")[0].value;
        var password = $("#input-password")[0].value;
        $.post("api/AddAttack", { "date": date, "location": location, "description": description, "password": password }, function () {
            alert("success!");
        });
    });
}

function inspectAttack(id) {
    var pw = prompt("Passwrd?");
    var jqxhr = $.get(`api/GetAttack?id=${id}&password=${pw}`,
        function (data) {
            console.log(data);
            if (data.attack) {
                alert(`Date: ${data.attack.attackDate}\nLocation: ${data.attack.location}`);
            }
        });
}