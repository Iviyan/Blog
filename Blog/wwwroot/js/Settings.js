// ----- 

/*let navTabTriggerList = [].slice.call(
    document.querySelectorAll(`#nav-tab button[data-bs-toggle="tab"]`));
let navTabList = navTabTriggerList.map(function (navTabTriggerEl) {
    return new bootstrap.Tab(navTabTriggerEl);
});

let startPage = location.pathname.split('/').filter(p=>p).slice(-1)[0];
bootstrap.Tab.getInstance(navTabTriggerList.find(t => t.attributes.href?.value === startPage))?.show();

for (let tab of navTabTriggerList) {
    tab.addEventListener('shown.bs.tab', function (event) {
        history.replaceState({}, null, event.target.attributes.href.value);
    });
}*/

// ----- Bootstrap from validation

(function () {
    'use strict'

    let forms = document.querySelectorAll('.needs-validation')

    Array.prototype.slice.call(forms)
        .forEach(function (form) {
            form.addEventListener('submit', function (event) {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                }

                form.classList.add('was-validated')
            }, false)
        })
})()

// ----- CkEditor initialization

ClassicEditor
    .create(document.querySelector('#aboutMeInput'))
    .then(editor => {
        window.editor = editor;
    })
    .catch(error => {
        console.error('There was a problem initializing the editor.', error);
    });

// ----- Avatar change modal

let modal = document.getElementById('avatarChangeModal');
let bsModal = new bootstrap.Modal(modal);
let settingsAvatar = document.getElementById('settingsAvatar');
let avatarModalImage = document.getElementById('avatarModalImage');
let avatarFileInput = document.getElementById('avatarFileInput');
let cropper;

let tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
let tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
});

let imageType, imageTypeIsPng;

avatarFileInput.addEventListener('change', function (e) {
    let files = e.target.files;
    let done = function (url) {
        avatarModalImage.src = url;
        bsModal.show();
    };
    let reader;
    let file;

    if (files && files.length > 0) {
        file = files[0];
        imageType = file.type;
        imageTypeIsPng = imageType === "image/png";

        if (URL) {
            done(URL.createObjectURL(file));
        } else if (FileReader) {
            reader = new FileReader();
            reader.onload = function (e) {
                done(reader.result);
            };
            reader.readAsDataURL(file);
        }
    }
});

modal.addEventListener('shown.bs.modal', function () {
    cropper = new Cropper(avatarModalImage, {
        aspectRatio: 1,
        viewMode: 1
    });
});

modal.addEventListener('hidden.bs.modal', function () {
    cropper.destroy();
    avatarFileInput.value = '';
});

function showErrorAlertInModal(error) {
    modal.querySelector(".modal-body").insertAdjacentHTML("afterend",
        `
<div class="alert alert-danger alert-dismissible d-flex m-3" role="alert">
  <svg class="bi flex-shrink-0 me-2" width="24" height="24" role="img" aria-label="Danger:"><use xlink:href="#exclamation-triangle-fill"></use></svg>
  <div>
    ${error}
  </div>
  <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
</div>`
    );
}

document.getElementById('sendAvatar').addEventListener('click', function () {
    let canvas;

    if (cropper) {
        canvas = cropper.getCroppedCanvas({
            /*width: 300,
            height: 300,*/
            maxWidth: 500,
            maxHeight: 500
        });
        canvas.toBlob(async function (blob) {
            let formData = new FormData();
            formData.append('file', blob, `avatar.${imageTypeIsPng ? 'png' : 'jpg'}`);
            
            try {
                let response = await fetch('/Settings/UploadAvatar', {
                    method: 'POST',
                    body: formData
                });

                let result = await response.json();
                console.log(result);

                if (result.error != 0) {
                    showErrorAlertInModal(result.urror)
                } else {
                    bsModal.hide();
                    settingsAvatar.src = result.url;
                    document.getElementById("headerUserAvatar").src = result.url;
                    document.getElementById("resetAvatarForm").classList.remove("d-none");
                }
            } catch(err) {
                showErrorAlertInModal("Неизвестная ошибка");
                console.log(err);
            }
        }, imageTypeIsPng ? imageType : "image/jpeg");
    }
});