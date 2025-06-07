let quill;
let endpoint;
let editorInstance;

const BlockEmbed = Quill.import('blots/block/embed');
class ImageBlot extends BlockEmbed {
    static create(value) {
        let node = super.create();
        if (!value || !value.src) {
            node.classList.add('unwanted');
            return node;
        }

        node.setAttribute('src', value.src);
        node.setAttribute('alt', value.alt);
        node.classList.add('quill-editor-image-border');
        return node;
    }

    static value(node) {
        return {
            src: node.getAttribute('src'),
            alt: node.getAttribute('alt')
        };
    }

    update(attrs) {
        if (attrs.src && attrs.alt) {
            this.domNode.setAttribute('src', attrs.src);
            this.domNode.setAttribute('alt', attrs.alt);
        }
        this.domNode.classList.add('quill-editor-image-border');
    }
}

ImageBlot.blotName = 'image';
ImageBlot.tagName = 'img';
Quill.register(ImageBlot, true);

window.ImageResize ? Quill.register('modules/imageResize', ImageResize) : undefined;

const toolbarOptions = [
    ['bold', 'italic', 'underline', 'strike'],
    ['blockquote', 'code-block'],
    ['link', 'image', 'video'],

    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
    [{ 'script': 'sub' }, { 'script': 'super' }],
    [{ 'indent': '-1' }, { 'indent': '+1' }],

    [{ 'size': ['small', false, 'large', 'huge'] }],
    [{ 'header': [1, 2, 3, 4, 5, 6, false] }],

    [{ 'color': [] }, { 'background': [] }],
    [{ 'font': [] }],
    [{ 'align': [] }],
];

export const initQuill = (quillElement, url, readOnly, placeholder, theme) => {
    endpoint = url;
    const modules = {
        toolbar: readOnly ? false : {
            container: toolbarOptions,
            handlers: { image: imageHandler },
        },
        imageResize: !readOnly ? {} : undefined
    };

    var options = {
        debug: 'info',
        theme: theme,
        modules: modules,
        placeholder: placeholder,
        readOnly: readOnly,
        formats: !readOnly ? [
            'bold', 'italic', 'underline', 'strike', 'blockquote', 'code-block', 'list',
            'header', 'script', 'size', 'color', 'background', 'align', 'font', 'image',
            'video', 'indent', 'link'
        ] : []
    };

    quill = new window.Quill(quillElement, options);
    quillElement.classList.toggle('hide-toolbar', readOnly);

    quill.root.addEventListener('drop', async (event) => {
        event.preventDefault();
        event.stopPropagation();
        droppedFileHandler(event);
    });
}

const droppedFileHandler = async (event) => {
    const file = event.dataTransfer.files[0];
    if (!file || !isValidImage(file)) {
        return console.error('Invalid image file');
    }

    if (file.size > 2 * 1024 * 1024) {
        return console.error('Image size exceeds 2MB');
    }

    const imageBase64 = await getBase64Image(event);
    imageBase64 === null ? console.error('Error reading file') : insertImage(false, imageBase64, file.name);
}

const getBase64Image = (event) => {
    return new Promise((resolve) => {
        const reader = new FileReader();
        const file = event.dataTransfer.files[0];
        reader.onload = (event) => {
            resolve(event.target.result);
        };

        reader.onerror = () => {
            console.error('Error reading file');
            resolve(null);
        };

        reader.readAsDataURL(file);
    });
}

const removeInvalidImageTags = () => {
    quill.root.querySelectorAll('img.unwanted')
        .forEach(img => img.remove());
};

const isValidImage = (file) => ['image/gif', 'image/jpeg', 'image/jpg', 'image/png', 'image/svg+xml', 'image/webp', 'image/bmp', 'image/vnd.microsoft.icon'].includes(file.type);

const getRange = () => quill.getSelection() || { index: quill.getLength(), length: 0 };

export const setQuillHTML = (html) => {
    if (quill && html) {
        const currentHTML = quill.root.innerHTML;
        quill.root.innerHTML = `${currentHTML}${html}`;
        setNetLine();
    }
}

export const getQuillHTML = async () => {
    if (quill) {
        const html = quill.root.innerHTML;
        const chunkSize = 16000;
        const chunks = splitContentIntoChunks(html, chunkSize);
        const promises = chunks.map((chunk, i) => editorInstance.invokeMethodAsync('DataReceived', chunk, i, chunks.length));
        await Promise.all(promises);
    }
}

const splitContentIntoChunks = (content, chunkSize) => {
    const chunks = [];
    for (let i = 0; i < content.length; i += chunkSize) {
        chunks.push(content.slice(i, i + chunkSize));
    }
    return chunks;
};

export const setQuillContents = (content) => {
    if (quill) {
        const Delta = Quill.import('delta');
        let jsonObject = JSON.parse(content);
        let delta = new Delta(jsonObject.ops);
        quill.setContents(delta);
        setNetLine();
    }
};

export const getQuillContents = async () => {
    if (quill) {
        const delta = quill.getContents();
        const content = JSON.stringify(delta);
        const chunkSize = 16000;
        const chunks = splitContentIntoChunks(content, chunkSize);
        const promises = chunks.map((chunk, i) => editorInstance.invokeMethodAsync('DataReceived', chunk, i, chunks.length));
        await Promise.all(promises);
    }
};

const imageHandler = async () => {
    await editorInstance.invokeMethodAsync('ShowImageModal');
}

export const fetchImagesAsync = async () => {
    try {
        let response = await fetch(endpoint);
        let images = await response.json();
        return JSON.stringify(images.map(image => ({
            src: image.trim(),
            alt: image.split('/').pop()
        })));
    } catch (error) {
        console.error('Error fetching images');
        return JSON.stringify([]);
    }
}
export const insertImage = (isUrl, imageData, name) => {
    const range = getRange();
    const image = imageData.trim();
    if (isUrl) {
        name = image.split('/').pop().split('?')[0] || 'image';
        if (!/\.(jpeg|jpg|gif|png|webp|bmp)$/i.test(name)) {
            return console.error('Invalid image URL');
        }
    }
    quill.insertEmbed(range.index, 'image', { src: image, alt: name });
    setNetLine();
    setTimeout(removeInvalidImageTags(), 50);
}

const setNetLine = () => {
    const range = getRange();
    quill.insertText(range.index + 1, '\n');
    quill.setSelection(range.index + 1, 0);
}

export const registerEditorInstance = (dotnetHelper) => {
    editorInstance = dotnetHelper;
}