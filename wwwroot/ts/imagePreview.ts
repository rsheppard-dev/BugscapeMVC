class ImagePreview {
	inputs: NodeListOf<HTMLInputElement>;
	outputs: NodeListOf<HTMLImageElement>;
	saveButton: HTMLButtonElement;
	imageSource?: string;

	constructor(imageSource?: string) {
		this.inputs = document.querySelectorAll('[data-image-input]');
		this.outputs = document.querySelectorAll('[data-image-output]');
		this.saveButton = document.querySelector('[data-save-button]') as HTMLButtonElement;
		this.imageSource = this.decodeHtml(imageSource);
	}

	init() {
		if (this.saveButton) this.saveButton.disabled = true;

		this.inputs.forEach((input, index) => {
			input.addEventListener('click', () => {
				if (this.imageSource) this.outputs[index].src = this.imageSource;
				if (this.saveButton) this.saveButton.disabled = true;
				input.value = '';
			});
			
			input.addEventListener('change', (event) => {
				const file = (event.target as HTMLInputElement)?.files?.[0];
				if (file) {
					const reader = new FileReader();
					reader.onload = () => {
						if (this.outputs[index]) {
							this.outputs[index].src = reader.result as string;
							if (this.saveButton) this.saveButton.disabled = false;
						}
					};
					reader.readAsDataURL(file);
				}
			});
		});
	}

	decodeHtml(html: string = '') {
		var txt = document.createElement("textarea");
		txt.innerHTML = html;
		return txt.value;
	}
}
