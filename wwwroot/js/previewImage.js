const imageInput = document.getElementById('imageInput');

imageInput.addEventListener('change', previewImage);

function previewImage() {
	const file = this.files[0];
	const reader = new FileReader();
	reader.onload = function (event) {
		document.getElementById('imageFileData').src = event.target.result;
	};
	reader.readAsDataURL(file);
}
