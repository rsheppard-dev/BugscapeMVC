function previewImage(inputId: string, outputId: string): void {
	const imageInput = document.getElementById(inputId) as HTMLInputElement;
	const imageOutput = document.getElementById(outputId) as HTMLImageElement;

	imageInput.addEventListener('change', function () {
		const file = this.files?.[0];

		if (file) {
			const reader = new FileReader();
			reader.onload = function (event) {
				if (imageOutput.src && event.target?.result) {
					imageOutput.src = event.target.result.toString();
				}
			};
			reader.readAsDataURL(file);
		}
	});
}
