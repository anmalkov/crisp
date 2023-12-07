export const fetchThreatModels = async () => {
    const response = await fetch('api/threatmodels');
    const result = await response.json();
    if (response.status !== 200) {
        throw Error(result.detail);
    }
    return result;
}

export const fetchThreatModelCategory = async () => {
    const response = await fetch('api/threatmodels/categories');
    const result = await response.json();
    if (response.status !== 200) {
        throw Error(result.detail);
    }
    return result;
}

export const fetchThreatModelReport = async (id) => {
    const response = await fetch(`api/threatmodels/${id}/report`);
    if (response.status !== 200) {
        const result = await response.json();
        throw Error(result.detail);
    }
    const report = await response.text();
    return report;
}

export const createThreatModel = async (threatModel, files) => {
    const request = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(threatModel)
    };
    const response = await fetch('api/threatmodels', request);
    if (response.status !== 200) {
        const result = await response.json();
        throw Error(result.detail);
    }
    const result = await response.json();
    await uploadFiles(result.id, files);
}

export const updateThreatModel = async (id, threatModel, files) => {
    const request = {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(threatModel)
    };
    const response = await fetch(`api/threatmodels/${id}`, request);
    if (response.status !== 200) {
        const result = await response.json();
        throw Error(result.detail);
    }
    await uploadFiles(id, files);
}

export const fetchThreatModelFiles = async (id, images) => {
    const files = [];
    if (!images || images.length === 0) {
        return files;
    }
    for (var i = 0; i < images.length; i++) {
        const image = images[i];
        const response = await fetch(`api/threatmodels/${id}/file/${image.value}`);
        if (response.status !== 200) {
            const result = await response.json();
            throw Error(result.detail);
        }
        const fileContent = await response.blob();
        files.push({ type: image.key, name: image.value, content: fileContent });
    }
    return files;
}

export const uploadFiles = async (id, files) => {
    const filesToUpload = files.filter(f => f.file);
    if (filesToUpload.length === 0) {
        return;
    }
    const formData = new FormData();
    filesToUpload.forEach(f => formData.append('file', f.file));
    const request = {
        method: 'POST',
        body: formData,
        dataType: "jsonp"
    };
    const response = await fetch(`api/threatmodels/${id}/upload`, request);
    if (response.status !== 200) {
        const result = await response.json();
        throw Error(result.detail);
    }
}

export const deleteThreatModel = async (id) => {
    const request = {
        method: 'DELETE'
    };
    const response = await fetch(`api/threatmodels/${id}`, request);
    if (response.status !== 200) {
        const result = await response.json();
        throw Error(result.detail);
    }
}