class ImagePreview {
	inputs: NodeListOf<HTMLInputElement>;
	outputs: NodeListOf<HTMLImageElement>;
	saveButton: HTMLButtonElement;

	constructor() {
		this.inputs = document.querySelectorAll('[data-image-input]');
		this.outputs = document.querySelectorAll('[data-image-output]');
		this.saveButton = document.querySelector('[data-save-button]') as HTMLButtonElement;
	}

	init() {
		if (this.saveButton) this.saveButton.disabled = true;

		this.inputs.forEach((input, index) => {
			input.addEventListener('change', (event) => {
				const file = (event.target as HTMLInputElement)?.files?.[0]; // Add null check here
				if (file) {
					const reader = new FileReader();
					reader.onload = () => {
						if (this.outputs[index]) {
							this.outputs[index].src = reader.result as string;
						}
					};
					reader.readAsDataURL(file);
					
					if (this.saveButton) this.saveButton.disabled = false;
				}
			});
		});
	}
}
