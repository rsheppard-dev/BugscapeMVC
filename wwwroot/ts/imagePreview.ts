class ImagePreview {
	input: HTMLInputElement;
	output: HTMLImageElement;

	constructor(input: HTMLInputElement, output: HTMLImageElement) {
		this.input = input;
		this.output = output;
	}

	init() {
		this.input.addEventListener('change', () => {
			const file = this.input.files?.[0];

			if (file) {
				const reader = new FileReader();
				reader.onload = (event) => {
					if (this.output.src && event.target?.result) {
						this.output.src = event.target.result.toString();
					}
				};
				reader.readAsDataURL(file);
			}
		});		
	}
}
