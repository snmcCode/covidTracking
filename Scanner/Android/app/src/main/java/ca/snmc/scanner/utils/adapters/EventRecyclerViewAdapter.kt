package ca.snmc.scanner.utils.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import ca.snmc.scanner.R
import ca.snmc.scanner.models.EventListItem
import kotlinx.android.synthetic.main.layout_events_list_item.view.*

class EventRecyclerViewAdapter(
    var clickListener: OnEventItemClickListener
) : RecyclerView.Adapter<RecyclerView.ViewHolder>() {

    private var items : List<EventListItem> = ArrayList()

    private var selectedPosition: Int = -1 // None selected by default
    private var selectedItemTextColor : Int? = null

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RecyclerView.ViewHolder {
        return EventListItemViewHolder(
            LayoutInflater.from(parent.context).inflate(R.layout.layout_events_list_item, parent, false)
        )
    }

    override fun onBindViewHolder(holder: RecyclerView.ViewHolder, position: Int) {

        when (holder) {

            is EventListItemViewHolder -> {
                holder.bind(items[position], this.selectedPosition, clickListener)
            }

        }

    }

    override fun getItemCount(): Int {
        return items.size
    }

    fun submitList(eventList: List<EventListItem>) {
        items = eventList
    }

    fun selectItem(selectedPosition: Int) {
        this.selectedPosition = selectedPosition
        notifyDataSetChanged()
    }

    class EventListItemViewHolder constructor(
        itemView: View
    ) : RecyclerView.ViewHolder(itemView) {
        private val eventListItemTextView : TextView = itemView.event_list_item_text

        fun bind(item : EventListItem, selectedPosition: Int, action: OnEventItemClickListener) {
            eventListItemTextView.text = item.name

            if (adapterPosition == selectedPosition) {
                eventListItemTextView.setBackgroundResource(R.drawable.event_item_background_selected)
            } else {
                eventListItemTextView.setBackgroundResource(R.drawable.event_item_background)
            }

            itemView.setOnClickListener {
                action.onItemClick(item, adapterPosition)
            }
        }

    }

}

interface OnEventItemClickListener {
    fun onItemClick(item: EventListItem, position: Int)
}