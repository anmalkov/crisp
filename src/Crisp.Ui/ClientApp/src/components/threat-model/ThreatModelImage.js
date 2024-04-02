import React from 'react';
import { CloseButton } from 'reactstrap';
import './ThreatModelImage.css';

const ThreatModelImage = ({ images, setImages, type }) => {

    function onImageChange(type, e) {
        const newImages = images.filter(i => i.type != type);
        if (e && e.target && e.target.files[0]) {
            const file = e.target.files[0];
            newImages.push({ type: type, fileName: file.name, file: file, url: URL.createObjectURL(file) });
        } else {
            const element = document.getElementById(`image-${type}`);
            element.value = null;
        }
        setImages(newImages);
    }

    return (
        <div>
            <input
                id={`image-${type}`}
                className="form-control mb-3"
                type="file"
                accept="image/*"
                onChange={(e) => onImageChange(type, e)}
            />
            {images.filter(i => i.type === type).map(i => (
                <div key={i.type} className="position-relative">
                    <img className="diagram mb-3" src={i.url} alt={`${type} diagram`} />
                    <CloseButton className="position-absolute top-0 end-0 border border-dark bg-white" onClick={() => onImageChange(type)} />
                </div>
            ))}
        </div>
    );
};

export default ThreatModelImage;
