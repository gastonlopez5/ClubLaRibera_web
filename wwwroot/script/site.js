// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    var placeholderElement = $('#modal-placeholder');
    var placeholderElementRecuperar = $('#modal-placeholder-recuperar');

    $('a[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');
        });
    });

    placeholderElement.on('click', '[data-save="modal"]', function (event) {
        event.preventDefault();

        var formData = new FormData();
        if ($('#Archivo').length) {
            formData.append("Archivo", $('#Archivo')[0].files[0]); //append the image file
        }
        
        var form = $(this).parents('.modal').find('form');
        var actionUrl = form.attr('action');

        var other_data = $('form').serializeArray();
        $.each(other_data, function (key, input) { //append other input value
            formData.append(input.name, input.value);
        });

        $.ajax({
            type: "POST",
            url: actionUrl,
            data: formData,
            contentType: false, // Not to set any content header
            processData: false, // Not to process data
            success: function (res) {
                var newBody = $('.modal-body', res);
                placeholderElement.find('.modal-body').replaceWith(newBody);

                var error = newBody.find('[name="IsError"]').val();
                var success = newBody.find('[name="IsSuccess"]').val();
                var valid = newBody.find('[name="IsValid"]').val() == "True";

                if (error != null) {
                    if (error.length != 0) {
                        alertify.set('notifier', 'position', 'top-center');
                        alertify.error(newBody.find('[name="IsError"]').val());
                    } else if (valid) {
                        alertify.set('notifier', 'position', 'top-center');
                        alertify.success(success);
                        placeholderElement.find('.modal').modal('hide');
                    }
                } 
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                alert("Status: " + textStatus); alert("Error: " + errorThrown);
            }  
        })
    });

    placeholderElement.on("click", '[data-save="recuperar-modal"]', function (event) {
        event.preventDefault();

        var url = $(this).data('url');
        console.log(url);
        placeholderElement.find('.modal').modal('hide');

        $.get(url).done(function (data) {
            placeholderElementRecuperar.html(data);
            placeholderElementRecuperar.find('.modal').modal('show');
        });
    });

    placeholderElement.on('change', '[id="Archivo"]', function (event) {
        event.preventDefault();

        //Get count of selected files
        var countFiles = $(this)[0].files.length;

        var imgPath = $(this)[0].value;
        var extn = imgPath.substring(imgPath.lastIndexOf('.') + 1).toLowerCase();
        var image_holder = $("#imagePreview");
        image_holder.empty();

        if (extn == "gif" || extn == "png" || extn == "jpg" || extn == "jpeg") {
            if (typeof (FileReader) != "undefined") {

                //loop for each file selected for uploaded.
                for (var i = 0; i < countFiles; i++) {

                    var reader = new FileReader();
                    reader.onload = function (e) {
                        $("<img />", {
                            "src": e.target.result,
                            "class": "thumb-image",
                            "width": "200px",
                            "height": "auto"
                        }).appendTo(image_holder);
                    }

                    image_holder.show();
                    reader.readAsDataURL($(this)[0].files[i]);
                }

            } else {
                alertify.set('notifier', 'position', 'top-center');
                alertify.error('Este buscador no soporta FileReader');
            }
        } else {
            alertify.set('notifier', 'position', 'top-center');
            alertify.error('Por favor, seleccione solo imagenes');
        }
    });
});





