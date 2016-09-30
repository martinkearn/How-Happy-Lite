﻿$(document).on('change', '.btn-file :file', function () {
    var input = $(this),
      numFiles = input.get(0).files ? input.get(0).files.length : 1,
      label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    input.trigger('fileselect', [numFiles, label]);
});

$(document).ready(function () {
    //set initial form visibility
    $("#form-file-input").show();
    $("#form-submit-button").show();
    $("#intro-text").show();
    $("#Emotion").hide();

    $("#upload-button").click(function (evt) {
        SubmitForm();
    });

    $("#Emotion").change(function (evt) {
        SubmitForm();
    });

    function SubmitForm() {
        //Display image
        ShowImage();

        //get form data
        var fd = new FormData();
        var file_data = $('input[type="file"]')[0].files; // for multiple files
        for (var i = 0; i < file_data.length; i++) {
            fd.append("file_" + i, file_data[i]);
        }
        var other_data = $('form').serializeArray();
        $.each(other_data, function (key, input) {
            fd.append(input.name, input.value);
        });

        //get data
        $.ajax({
            type: "POST",
            url: "/home/Result",
            contentType: false,
            processData: false,
            data: fd,
            success: function (response) {
                ProcessResult(response);
            },
            error: function () {
                alert("There was error uploading files!");
            }
        });
    }

    function ShowImage() {
        //clear existing result
        $("#result").html("");

        //get selected image
        var input = document.getElementById('file');

        //show selected image
        var file = input.files[0];
        var fr = new FileReader();
        fr.onload = createImage;
        fr.readAsDataURL(file);

        function createImage() {
            //add image to div
            $('#result').append($('<img>', { id: 'photo', src: fr.result }))

            //set the result div to the width of the image so that the rectangles line up
            $('#result').css("width", $('#photo').width());
        }
    }

    function ProcessResult(response) {
        var dataString = JSON.stringify(response);
        var data = JSON.parse(dataString);

        $("#resultDetails").html("<span>We found " + data.length + " faces</span>");

        //draw rectangle for each face
        $.each(data, function (index, value) {

            var rect = document.createElement('div');
            rect.className = "rect";
            rect.style.height = value.faceRectangle.height + "px";
            rect.style.width = value.faceRectangle.width + "px";
            rect.style.left = value.faceRectangle.left + "px";
            rect.style.top = value.faceRectangle.top + "px";
            rect.id = "rect" + index;

            var rank = document.createElement('div');
            rank.className = "rank";
            rank.innerHTML = "#" + (index + 1);

            rect.appendChild(rank);

            $('#result').append(rect);

            //add popover
            var popoverBody = "Happiness: " + Number((value.scores.happiness).toFixed(2))
                + "<br>Fear: " + Number((value.scores.fear).toFixed(2))
                + "<br>Anger: " + Number((value.scores.anger).toFixed(2))
                + "<br>Contempt: " + Number((value.scores.contempt).toFixed(2))
                + "<br>Disgust: " + Number((value.scores.disgust).toFixed(2))
                + "<br>Neutral: " + Number((value.scores.neutral).toFixed(2))
                + "<br>Sadness: " + Number((value.scores.sadness).toFixed(2))
                + "<br>Surprise: " + Number((value.scores.surprise).toFixed(2));
            $('#rect' + index).popover({
                title: "#" + (index + 1),
                content: popoverBody,
                html: "true",
                trigger: "click"
            });
        });
    }

});
