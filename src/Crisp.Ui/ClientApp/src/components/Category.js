import React, { useState } from 'react';
import { ListGroupItem, Badge, Input } from 'reactstrap';
import Recommendation from './Recommendation';

const Category = ({ category, level, isSelected, toggleSelectability }) => {

    const [isCollapsed, setIsCollapsed] = useState(false);

    if (!level) {
        level = 0;
    }

    const getPaddingLeft = () => {
        return (level * 30) + 15;
    }

    const calculateRecommendationsCount = (category, onlySelected) => {
        let count = onlySelected
            ? category.recommendations.filter(r => isSelected(r.id)).length
            : category.recommendations.length;
        category.children.forEach(c => count += calculateRecommendationsCount(c, onlySelected));
        return count;
    }

    const toggleIsCollapsed = (e) => {
        if (e.target.tagName === "INPUT") {
            return;
        }
        setIsCollapsed(!isCollapsed);
    }

    const toggleIsSelect = (category) => {
        toggleSelectability(category);
    }

    const categorySelected = isSelected(category.id);
    const recommendationsCount = calculateRecommendationsCount(category, false);
    const selectedRecommendationsCount = calculateRecommendationsCount(category, true);

    return (
        <>
            <ListGroupItem className="d-flex justify-content-between align-items-center" style={{ paddingLeft: getPaddingLeft() + 'px' }} action tag="button" onClick={toggleIsCollapsed}>
                <div>
                    <Input className="form-check-input me-3" type="checkbox" checked={categorySelected} onChange={() => toggleIsSelect(category)} />
                    <b>{category.name}</b>
                </div>
                <Badge color={categorySelected ? 'primary' : 'secondary'}>
                    {categorySelected ? selectedRecommendationsCount : recommendationsCount}
                </Badge>
            </ListGroupItem>
            {!isCollapsed ? (
                <>
                    {category.children.map(c => (
                        <Category key={c.id} category={c} level={level + 1} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    ))}
                    {category.recommendations.map(r => (
                        <Recommendation key={r.id} recommendation={r} level={level + 1} isSelected={isSelected} toggleSelectability={toggleSelectability} />
                    ))}
                </>
            ) : null}
        </>
    )
}

export default Category;